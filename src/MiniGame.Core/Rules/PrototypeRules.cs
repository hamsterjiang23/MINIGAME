using System.Collections.Generic;
using System.Linq;

namespace MiniGame.Core
{
    // Development fixtures only. These values are not approved final design parameters.
    public static class PrototypeRules
    {
        public const string Priest = "priest";
        public const string Child = "child";
        public const string FormerRuler = "former-ruler";
        public const string Doctor = "battlefield-doctor";
        public const string Diplomat = "diplomat";

        public static GameRules Create()
        {
            var patients = new[]
            {
                new PatientDefinition(Child, "米洛", 1, 0, 3, PatientPresence.Deceased),
                new PatientDefinition(FormerRuler, "薇拉·沃斯", 1, 0, 3, PatientPresence.Deceased),
                new PatientDefinition(Doctor, "埃德温", 1, 0, 3, PatientPresence.Deceased),
                new PatientDefinition(Diplomat, "阿德里安", 1, 0, 3, PatientPresence.TreatmentEnded),
                new PatientDefinition(Priest, "乔纳", 2, 0, 3, PatientPresence.Deceased, 1, true)
            };
            var days = Enumerable.Range(1, 3).Select(day => new DayDefinition(day, 1,
                patients.Where(p => p.FirstDay <= day).Select(p => CreateMeeting(p.Id, day))));
            return new GameRules("prototype-v1", patients, days,
                new[] { EndingType.PriestAttack, EndingType.LockedInRoom, EndingType.TakenAway });
        }

        public static MeetingDefinition CreateMeeting(string patientId, int day, int normalRounds = 4,
            int specialThreshold = 20, int releaseThreshold = 30)
        {
            var prefix = $"day{day}.{patientId}";
            var normal = Enumerable.Range(1, normalRounds).Select(index => CreateRound($"{prefix}.normal.{index}"));
            var reactions = new Dictionary<Disposition, string>
            {
                [Disposition.Treat] = $"{prefix}.reaction.treat",
                [Disposition.Purify] = $"{prefix}.reaction.purify",
                [Disposition.Release] = $"{prefix}.reaction.release"
            };
            return new MeetingDefinition(patientId, normal, CreateRound($"{prefix}.special"),
                specialThreshold, releaseThreshold, reactions);
        }

        private static RoundDefinition CreateRound(string nodeId) => new RoundDefinition(nodeId, new[]
        {
            new ResponseDefinition("empathize", $"{nodeId}.branch.empathize", 10),
            new ResponseDefinition("neutral", $"{nodeId}.branch.neutral", 0),
            new ResponseDefinition("pressure", $"{nodeId}.branch.pressure", -10)
        });
    }
}
