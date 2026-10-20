"""Fit a closed CRT overlay to the accepted monitor's recessed, curved glass.

Run with the already installed NumPy/trimesh Python environment. This reads the
source GLB only; it never changes the source room, monitor, texture, or material.
"""
from __future__ import annotations

from collections import deque
from dataclasses import dataclass
from hashlib import sha256
from pathlib import Path
import json
import math

import numpy as np
from numpy.typing import NDArray
import trimesh

FloatArray = NDArray[np.float64]
BoolArray = NDArray[np.bool_]
ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
SOURCE = ROOT / "Art/MainRoom/reference/MainRoomReference.glb"
RESOURCE = ROOT / "UnityProject/Assets/MiniGame/Resources/MiniGame/CrtScreenFit.json"


@dataclass
class Hit:
    point: FloatArray
    normal: FloatArray
    rgb: tuple[int, int, int]


class Surface:
    def __init__(self, scene: trimesh.Scene) -> None:
        transform, geometry = scene.graph["CRT_Monitor_Tripo"]
        self.transform = np.asarray(transform, dtype=float)
        self.inverse = np.linalg.inv(self.transform)
        monitor = scene.geometry[geometry]
        world = monitor.copy()
        world.apply_transform(self.transform)
        self.mesh = world
        # Keep all potentially visible front triangles, including the bezel.
        keep = (world.triangles[:, :, 2].min(axis=1) < .23)
        self.tri = world.triangles[keep]
        self.uv = monitor.visual.uv[monitor.faces[keep]]
        self.texture = monitor.visual.material.baseColorTexture
        self.a = self.tri[:, 0, :2]
        self.b = self.tri[:, 1, :2]
        self.c = self.tri[:, 2, :2]
        self.denominator = ((self.b[:, 1] - self.c[:, 1]) * (self.a[:, 0] - self.c[:, 0])
                            + (self.c[:, 0] - self.b[:, 0]) * (self.a[:, 1] - self.c[:, 1]))
        centers = world.triangles_center
        glass = ((centers[:, 0] > 2.875) & (centers[:, 0] < 3.135)
                 & (centers[:, 1] > .225) & (centers[:, 1] < .41)
                 & (centers[:, 2] < .22) & (world.face_normals[:, 2] < -.65))
        self.fit_points = centers[glass]
        self.coefficients = np.linalg.lstsq(self.basis(self.fit_points[:, :2]),
                                            self.fit_points[:, 2], rcond=None)[0]

    @staticmethod
    def basis(xy: FloatArray) -> FloatArray:
        x, y = xy[:, 0] - 3.004, xy[:, 1] - .315
        return np.column_stack((x*x, x*y, y*y, x, y, np.ones_like(x)))

    def glass_normal(self, x: float, y: float) -> FloatArray:
        a, b, c, d, e, _ = self.coefficients
        dx, dy = x-3.004, y-.315
        normal = np.array([2*a*dx+b*dy+d, b*dx+2*c*dy+e, -1.])
        return normal / np.linalg.norm(normal)

    def fallback(self, x: float, y: float) -> Hit:
        neighbors: list[Hit] = []
        for distance in (.0005, .001, .002):
            for angle in np.linspace(0, 2*math.pi, 8, endpoint=False):
                hit = self.cast(x + distance*math.cos(angle), y + distance*math.sin(angle))
                if hit is not None:
                    neighbors.append(hit)
        if len(neighbors) >= 8:
            points = np.array([hit.point for hit in neighbors])
            dx, dy = (points[:, 0]-x)*1000, (points[:, 1]-y)*1000
            basis = np.column_stack((dx*dx, dx*dy, dy*dy, dx, dy, np.ones_like(dx)))
            local = np.linalg.lstsq(basis, points[:, 2], rcond=None)[0]
            normal = np.mean([hit.normal for hit in neighbors], axis=0)
            normal /= np.linalg.norm(normal)
            return Hit(np.array([x, y, local[-1]]), normal, (0, 0, 0))
        a, b, c, d, e, _ = self.coefficients
        dx, dy = x - 3.004, y - .315
        z = float((self.basis(np.array([[x, y]])) @ self.coefficients)[0])
        normal = np.array([2*a*dx + b*dy + d, b*dx + 2*c*dy + e, -1.])
        return Hit(np.array([x, y, z]), normal / np.linalg.norm(normal), (0, 0, 0))

    def cast(self, x: float, y: float) -> Hit | None:
        with np.errstate(divide="ignore", invalid="ignore"):
            a = ((self.b[:, 1] - self.c[:, 1]) * (x - self.c[:, 0])
                 + (self.c[:, 0] - self.b[:, 0]) * (y - self.c[:, 1])) / self.denominator
            b = ((self.c[:, 1] - self.a[:, 1]) * (x - self.c[:, 0])
                 + (self.a[:, 0] - self.c[:, 0]) * (y - self.c[:, 1])) / self.denominator
            c = 1 - a - b
        ids = np.flatnonzero((a >= -1e-7) & (b >= -1e-7) & (c >= -1e-7))
        if not len(ids):
            return None
        weights = np.column_stack((a[ids], b[ids], c[ids]))
        depths = (weights * self.tri[ids, :, 2]).sum(axis=1)
        index = int(np.argmin(depths))
        face = ids[index]
        if depths[index] > .23:
            return None
        bary = weights[index]
        uv = bary @ self.uv[face]
        width, height = self.texture.size
        rgb = self.texture.getpixel((int(np.clip(uv[0], 0, 1)*(width-1)),
                                     int((1-np.clip(uv[1], 0, 1))*(height-1))))
        # The scan has reversed normals on internal folds; the measured smooth
        # glass derivative gives a consistent physical outward offset instead.
        normal = self.glass_normal(x, y)
        return Hit(np.array([x, y, depths[index]]), normal, tuple(int(n) for n in rgb[:3]))


def flood(mask: BoolArray, seed: tuple[int, int]) -> BoolArray:
    result = np.zeros_like(mask)
    queue: deque[tuple[int, int]] = deque([seed])
    while queue:
        row, col = queue.popleft()
        if row < 0 or col < 0 or row >= mask.shape[0] or col >= mask.shape[1]:
            continue
        if result[row, col] or not mask[row, col]:
            continue
        result[row, col] = True
        queue.extend(((row-1, col), (row+1, col), (row, col-1), (row, col+1)))
    return result


def close_holes(mask: BoolArray) -> BoolArray:
    padded = np.pad(mask, 1)
    outside = flood(~padded, (0, 0))
    return (~outside)[1:-1, 1:-1]


def rounded_x(y: float, bounds: list[float], radius: float) -> tuple[float, float]:
    left, bottom, right, top = bounds
    dy = max(bottom + radius - y, y - (top - radius), 0.)
    inset = radius - math.sqrt(max(radius*radius - dy*dy, 0.)) if dy else 0.
    return left + inset, right - inset


def vec3(value: FloatArray) -> dict[str, float]:
    return {key: round(float(value[i]), 9) for i, key in enumerate(("x", "y", "z"))}


def main() -> None:
    scene = trimesh.load(SOURCE, force="scene")
    surface = Surface(scene)
    xs = np.linspace(2.83, 3.18, 351)
    ys = np.linspace(.19, .445, 256)
    dark = np.zeros((len(ys), len(xs)), dtype=bool)
    colors: list[list[tuple[int, int, int]]] = []
    for row, y in enumerate(ys):
        color_row: list[tuple[int, int, int]] = []
        for col, x in enumerate(xs):
            hit = surface.cast(float(x), float(y))
            rgb = hit.rgb if hit else (155, 145, 120)
            color_row.append(rgb)
            dark[row, col] = hit is not None and max(rgb) < 120
        colors.append(color_row)
    # One continuous aperture silhouette. Filling enclosed scan/texture holes
    # prevents threshold noise from deleting isolated overlay triangles.
    aperture = close_holes(flood(dark, (110, 174)))
    central_rows = np.flatnonzero((ys > .24) & (ys < .39))
    lefts = [xs[np.flatnonzero(aperture[row])[0]] for row in central_rows]
    rights = [xs[np.flatnonzero(aperture[row])[-1]] for row in central_rows]
    central_cols = np.flatnonzero((xs > 2.90) & (xs < 3.11))
    bottoms = [ys[np.flatnonzero(aperture[:, col])[0]] for col in central_cols]
    tops = [ys[np.flatnonzero(aperture[:, col])[-1]] for col in central_cols]
    # Robust silhouettes exclude isolated dark scrapes on the beige housing.
    # The scan's top inner lip is uneven and textured with dark scratches.
    # Insets preserve that physical lip rather than covering its painted edge.
    bounds = [float(np.quantile(lefts, .9) + .004),
              float(np.quantile(bottoms, .9) + .004),
              float(np.quantile(rights, .1) - .004),
              float(np.quantile(tops, .1) - .008)]
    radius = .012
    nx, ny, offset = 65, 49, .00065
    vertices: list[dict[str, float]] = []
    normals: list[dict[str, float]] = []
    uvs: list[dict[str, float]] = []
    source_points: list[FloatArray] = []
    world_points: list[FloatArray] = []
    world_normals: list[FloatArray] = []
    missing: list[dict[str, float]] = []
    bad_boundary: list[dict[str, float]] = []
    original_depths: list[float] = []
    for row in range(ny):
        y = bounds[1] + row/(ny-1)*(bounds[3]-bounds[1])
        lo, hi = rounded_x(y, bounds, radius)
        for col in range(nx):
            x = lo + col/(nx-1)*(hi-lo)
            hit = surface.cast(x, y)
            if hit is None:
                missing.append({"x": x, "y": y})
                hit = surface.fallback(x, y)
            if (row in (0, ny-1) or col in (0, nx-1)) and max(hit.rgb) >= 120:
                bad_boundary.append({"x": x, "y": y, "maximumChannel": max(hit.rgb)})
            original_depths.append(float(hit.point[2]))
            # Preserve the aperture's exact projected footprint. A view-axis
            # offset cannot slide an edge vertex sideways onto the inner lip.
            point = hit.point + np.array([0., 0., -offset])
            local = (surface.inverse @ np.append(point, 1))[:3]
            local_normal = surface.inverse[:3, :3] @ hit.normal
            local_normal /= np.linalg.norm(local_normal)
            source_points.append(hit.point)
            world_points.append(point)
            world_normals.append(hit.normal)
            vertices.append(vec3(local))
            normals.append(vec3(local_normal))
            uvs.append({"x": round((x-bounds[0])/(bounds[2]-bounds[0]), 9),
                        "y": round((y-bounds[1])/(bounds[3]-bounds[1]), 9)})
    triangles: list[int] = []
    # glTF front normal is -Z. The source localToWorld has positive determinant.
    for row in range(ny-1):
        for col in range(nx-1):
            a = row*nx+col
            triangles.extend((a, a+nx, a+1, a+1, a+nx, a+nx+1))
    refinements: list[dict[str, int]] = []
    def add_sample(x: float, y: float) -> int:
        hit = surface.cast(x, y)
        if hit is None:
            hit = surface.fallback(x, y)
        point = hit.point + np.array([0., 0., -offset])
        local = (surface.inverse @ np.append(point, 1))[:3]
        normal = surface.inverse[:3, :3] @ hit.normal
        normal /= np.linalg.norm(normal)
        index = len(world_points)
        world_points.append(point)
        world_normals.append(hit.normal)
        source_points.append(hit.point)
        vertices.append(vec3(local))
        normals.append(vec3(normal))
        uvs.append({"x": round((x-bounds[0])/(bounds[2]-bounds[0]), 9),
                    "y": round((y-bounds[1])/(bounds[3]-bounds[1]), 9)})
        return index
    # A few scan ridges are sharper than the regular grid. Insert measured
    # points there rather than lifting the entire screen away from the glass.
    for _ in range(4):
        faces = np.asarray(triangles).reshape(-1, 3)
        positions = np.asarray(world_points)
        bad_faces: set[int] = set()
        bad_edges: dict[tuple[int, int], int] = {}
        edges = {tuple(sorted((int(tri[i]), int(tri[(i+1) % 3]))))
                 for tri in faces for i in range(3)}
        for index, tri in enumerate(faces):
            point = positions[tri].mean(axis=0)
            hit = surface.cast(float(point[0]), float(point[1]))
            if hit is not None and np.dot(point-hit.point, hit.normal) < -.00002:
                bad_faces.add(index)
        for edge in sorted(edges):
            point = positions[list(edge)].mean(axis=0)
            hit = surface.cast(float(point[0]), float(point[1]))
            if hit is not None and np.dot(point-hit.point, hit.normal) < -.00002:
                bad_edges[edge] = add_sample(float(point[0]), float(point[1]))
        if not bad_faces and not bad_edges:
            break
        refinements.append({"faces": len(bad_faces), "edges": len(bad_edges)})
        refined: list[int] = []
        for index, tri in enumerate(faces):
            perimeter: list[int] = []
            for i in range(3):
                a, b = int(tri[i]), int(tri[(i+1) % 3])
                perimeter.append(a)
                key = tuple(sorted((a, b)))
                if key in bad_edges:
                    perimeter.append(bad_edges[key])
            if index not in bad_faces and len(perimeter) == 3:
                refined.extend(int(n) for n in tri)
                continue
            center = positions[tri].mean(axis=0)
            middle = add_sample(float(center[0]), float(center[1]))
            for i in range(len(perimeter)):
                refined.extend((perimeter[i], perimeter[(i+1) % len(perimeter)], middle))
        triangles = refined
    helper_transform, helper_name = scene.graph["CRT screen emission"]
    helper = scene.geometry[helper_name].copy()
    helper.apply_transform(helper_transform)
    layout = json.loads((ROOT/"Art/MainRoom/reference/layout.json").read_text(encoding="utf-8"))
    def marker(name: str) -> dict[str, float]:
        x, y, z = layout["markers"][name]
        return vec3(np.array([x, z, -y]))
    payload = {
        "version": 1,
        "sourceSha256": sha256(SOURCE.read_bytes()).hexdigest(),
        "sourceMonitorNode": "CRT_Monitor_Tripo",
        "coordinateFrame": "vertices/normals: reference GLTF WORLD, metres, right handed Y-up. Map world through the four exported Reference markers before InverseTransformPoint into Unity screen mesh. Original monitor mesh-local coordinates can be reconstructed using the inverse sourceLocalToGltfWorldRowMajor matrix.",
        "sourceFrame": {"origin": marker("ReferenceOrigin"),
                        "right": marker("ReferenceRight"),
                        "up": marker("ReferenceUp"),
                        "depth": marker("ReferenceDepth")},
        "originalMonitorWorldBounds": {"min": vec3(surface.mesh.bounds[0]),
                                       "max": vec3(surface.mesh.bounds[1])},
        "sourceLocalToGltfWorldRowMajor": surface.transform.reshape(-1).tolist(),
        "sourceLocalBoundsMinimum": vec3(scene.geometry[scene.graph["CRT_Monitor_Tripo"][1]].bounds[0]),
        "sourceLocalBoundsMaximum": vec3(scene.geometry[scene.graph["CRT_Monitor_Tripo"][1]].bounds[1]),
        "sourceHelperLocalToGltfWorldRowMajor": np.asarray(helper_transform).reshape(-1).tolist(),
        "apertureBoundsGltfXY": bounds,
        "cornerRadiusMetres": radius,
        "outwardOffsetMetres": offset,
        "offsetAxisGltfWorld": {"x": 0., "y": 0., "z": -1.},
        "columns": nx, "rows": ny,
        "vertices": np.asarray(world_points).reshape(-1).round(9).tolist(),
        "normals": np.asarray(world_normals).reshape(-1).round(9).tolist(),
        "uv": [component for uv in uvs for component in (uv["x"], uv["y"])],
        "triangles": triangles,
    }
    RESOURCE.parent.mkdir(parents=True, exist_ok=True)
    RESOURCE.write_text(json.dumps(payload, separators=(",", ":")), encoding="utf-8")
    gaps: list[float] = []
    edge_gaps: list[float] = []
    gap_missing = 0
    edge_missing = 0
    penetrations: list[dict[str, object]] = []
    world_array = np.asarray(world_points)
    face_array = np.asarray(triangles).reshape(-1, 3)
    edge_pairs = {tuple(sorted((int(tri[i]), int(tri[(i+1) % 3]))))
                  for tri in face_array for i in range(3)}
    for tri in face_array:
        point = world_array[tri].mean(axis=0)
        actual = surface.cast(float(point[0]), float(point[1]))
        if actual is None:
            gap_missing += 1
            continue
        gap = float(np.dot(point-actual.point, actual.normal))*1000
        gaps.append(gap)
        if gap < -.02:
            penetrations.append({"kind": "center", "point": point.tolist(), "source": actual.point.tolist(), "gapMillimetres": gap})
    for edge in sorted(edge_pairs):
        point = world_array[list(edge)].mean(axis=0)
        actual = surface.cast(float(point[0]), float(point[1]))
        if actual is None:
            edge_missing += 1
            continue
        gap = float(np.dot(point-actual.point, actual.normal))*1000
        edge_gaps.append(gap)
        if gap < -.02:
            penetrations.append({"kind": "edge", "point": point.tolist(), "source": actual.point.tolist(), "gapMillimetres": gap})
    report = {
        "source": str(SOURCE.relative_to(ROOT)), "sourceSha256": payload["sourceSha256"],
        "resource": str(RESOURCE.relative_to(ROOT)),
        "resourceSha256": sha256(RESOURCE.read_bytes()).hexdigest(),
        "generatorSha256": sha256(Path(__file__).read_bytes()).hexdigest(),
        "method": "Continuous filled aperture silhouette; conservative inset rounded rectangle; actual front mesh ray depths with 0.65mm front/view-axis offset, preserving the projected aperture edge. Only scan holes interpolate nearby valid surface samples (global measured quadratic is emergency fallback).",
        "vertices": len(vertices), "triangles": len(triangles)//3,
        "apertureBoundsGltfXY": bounds, "cornerRadiusMetres": radius,
        "surfaceDepthRangeGltfZ": [min(original_depths), max(original_depths)],
        "depthVariationMillimetres": 1000*(max(original_depths)-min(original_depths)),
        "sourceHelperGltfWorldBounds": helper.bounds.tolist(),
        "quadraticFitCenterGltfXY": [3.004, .315],
        "quadraticFitCoefficientsXX_XY_YY_X_Y_Constant": surface.coefficients.tolist(),
        "sourceFitTriangles": len(surface.fit_points),
        "adaptiveRefinements": refinements,
        "scanHoleFallbackCount": len(missing), "scanHoleFallbackLocationsGltfXY": missing,
        "boundaryBrightBezelViolations": bad_boundary,
        "boundarySamples": 2*nx + 2*(ny-2),
        "triangleCenterGapMillimetresQuantiles0_1_50_99_100": np.quantile(gaps, [0, .01, .5, .99, 1]).tolist(),
        "triangleCentersBelowSource": sum(gap < -.02 for gap in gaps),
        "triangleCenterSourceScanHoles": gap_missing,
        "edgeMidpointGapMillimetresQuantiles0_1_50_99_100": np.quantile(edge_gaps, [0, .01, .5, .99, 1]).tolist(),
        "edgeMidpointsBelowSource": sum(gap < -.02 for gap in edge_gaps),
        "edgeMidpointsSourceScanHoles": edge_missing,
        "edgeMidpoints": len(edge_pairs),
        "penetrations": penetrations,
        "frameMapping": {"gltfWorldToBlenderWorld": "(X,Y,Z)->(X,-Z,Y)",
                         "referenceMinimumBlender": [-2.05606, -.69033, -.72788],
                         "normalDirection": "Original monitor local +Z / reference glTF world -Z",
                         "triangles": "Right handed glTF outward winding; reverse each triangle if target conversion reflects handedness."},
    }
    (HERE/"report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    # Code-native diagnostic: projected original texture with fitted footprint.
    scale = 2400
    def svg_x(x: float) -> float:
        return (x-xs[0])*scale+40
    def svg_y(y: float) -> float:
        return (ys[-1]-y)*scale+50
    lines = ['<svg xmlns="http://www.w3.org/2000/svg" width="940" height="740" viewBox="0 0 940 740">',
             '<rect width="940" height="740" fill="#171819"/>',
             '<text x="40" y="28" fill="white" font-family="sans-serif" font-size="18">Actual monitor front projection / fitted curved patch</text>']
    for row in range(0, len(ys), 3):
        for col in range(0, len(xs), 3):
            r, g, b = colors[row][col]
            lines.append(f'<rect x="{svg_x(float(xs[col])):.2f}" y="{svg_y(float(ys[row])):.2f}" width="7.3" height="7.3" fill="rgb({r},{g},{b})"/>')
    border: list[tuple[float, float]] = []
    for y in np.linspace(bounds[1], bounds[3], 80):
        lo, hi = rounded_x(float(y), bounds, radius)
        border.append((lo, float(y)))
    for y in np.linspace(bounds[3], bounds[1], 80):
        lo, hi = rounded_x(float(y), bounds, radius)
        border.append((hi, float(y)))
    path = " ".join(f'{svg_x(x):.2f},{svg_y(y):.2f}' for x, y in border)
    lines.extend((f'<polygon points="{path}" fill="none" stroke="#00eaff" stroke-width="2"/>',
                  '<text x="40" y="710" fill="#00eaff" font-family="sans-serif" font-size="16">Cyan: fitted footprint. Surface positions follow the source glass, not a flat plane.</text>', '</svg>'))
    (HERE/"front-diagnostic.svg").write_text("\n".join(lines), encoding="utf-8")
    print(json.dumps({key: report[key] for key in ("vertices", "triangles", "apertureBoundsGltfXY", "surfaceDepthRangeGltfZ", "depthVariationMillimetres", "scanHoleFallbackCount", "boundaryBrightBezelViolations")}, indent=2))
    if bad_boundary:
        raise SystemExit("Footprint touches bright housing; inspect source and fit before using.")
    if min(gaps + edge_gaps) < -.02:
        raise SystemExit("Overlay penetrates original surface at interior probes.")


if __name__ == "__main__":
    main()
