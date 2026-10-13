#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MiniGame.Core
{
    public sealed class PatientDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int FirstDay { get; }
        public int InitialTrust { get; }
        public int? TreatmentLimit { get; }
        public PatientPresence TreatmentOutcome { get; }
        public int TreatmentDelayDays { get; }
        public bool AttacksAfterRelease { get; }
        public int AttackDelayDays { get; }

        public PatientDefinition(string id, string displayName, int firstDay, int initialTrust,
            int? treatmentLimit, PatientPresence treatmentOutcome, int treatmentDelayDays = 0,
            bool attacksAfterRelease = false, int attackDelayDays = 1)
        {
            Id = RuleGuard.Identifier(id, nameof(id));
            DisplayName = RuleGuard.Identifier(displayName, nameof(displayName));
            if (firstDay < 1) throw new ArgumentOutOfRangeException(nameof(firstDay));
            if (treatmentLimit.HasValue && treatmentLimit.Value < 1)
                throw new ArgumentOutOfRangeException(nameof(treatmentLimit));
            if (treatmentOutcome != PatientPresence.Deceased && treatmentOutcome != PatientPresence.TreatmentEnded)
                throw new ArgumentException("Treatment must end in death or an unspecified termination.", nameof(treatmentOutcome));
            if (treatmentDelayDays < 0 || attackDelayDays < 1)
                throw new ArgumentException("Event delays must be non-negative, and release attacks occur on a later day.");
            FirstDay = firstDay; InitialTrust = initialTrust; TreatmentLimit = treatmentLimit;
            TreatmentOutcome = treatmentOutcome; TreatmentDelayDays = treatmentDelayDays;
            AttacksAfterRelease = attacksAfterRelease; AttackDelayDays = attackDelayDays;
        }
    }

    public sealed class DayDefinition
    {
        public int Day { get; }
        public int PurificationQuota { get; }
        public IReadOnlyList<MeetingDefinition> Meetings { get; }
        public DayDefinition(int day, int purificationQuota, IEnumerable<MeetingDefinition> meetings)
        {
            if (day < 1) throw new ArgumentOutOfRangeException(nameof(day));
            if (purificationQuota < 0) throw new ArgumentOutOfRangeException(nameof(purificationQuota));
            var list = RuleGuard.Copy(meetings, nameof(meetings));
            if (list.Select(m => m.PatientId).Distinct().Count() != list.Count)
                throw new ArgumentException("A patient may appear only once per day.", nameof(meetings));
            Day = day; PurificationQuota = purificationQuota; Meetings = list.AsReadOnly();
        }
    }

    public sealed class GameRules
    {
        public string RulesId { get; }
        public string Fingerprint { get; }
        public IReadOnlyList<PatientDefinition> Patients { get; }
        public IReadOnlyList<DayDefinition> Days { get; }
        public IReadOnlyList<EndingType> EndingPriority { get; }

        public GameRules(string rulesId, IEnumerable<PatientDefinition> patients,
            IEnumerable<DayDefinition> days, IEnumerable<EndingType> endingPriority)
        {
            RulesId = RuleGuard.Identifier(rulesId, nameof(rulesId));
            var profiles = RuleGuard.Copy(patients, nameof(patients));
            var calendar = RuleGuard.Copy(days, nameof(days));
            if (profiles.Count == 0 || profiles.Select(p => p.Id).Distinct().Count() != profiles.Count)
                throw new ArgumentException("Patients must have unique identifiers.", nameof(patients));
            if (calendar.Count == 0 || !calendar.Select(d => d.Day).SequenceEqual(Enumerable.Range(1, calendar.Count)))
                throw new ArgumentException("Days must be ordered and consecutive, starting at day 1.", nameof(days));
            if (profiles.Any(p => p.FirstDay > calendar.Count))
                throw new ArgumentException("Patient debut is outside the playable calendar.", nameof(patients));
            foreach (var day in calendar)
            foreach (var meeting in day.Meetings)
            {
                var patient = profiles.SingleOrDefault(p => p.Id == meeting.PatientId);
                if (patient == null || patient.FirstDay > day.Day)
                    throw new ArgumentException("Meeting refers to an unknown or not-yet-introduced patient.", nameof(days));
                if ((long)day.Day + Math.Max(patient.AttackDelayDays, patient.TreatmentDelayDays) > int.MaxValue)
                    throw new ArgumentException("Scheduled event day exceeds the integer range.", nameof(days));
            }
            if (endingPriority == null) throw new ArgumentNullException(nameof(endingPriority));
            var priority = endingPriority.ToList();
            if (priority.Count != 3 || priority.Distinct().Count() != 3 ||
                priority.Any(e => e == EndingType.None || !Enum.IsDefined(typeof(EndingType), e)))
                throw new ArgumentException("Every supported ending must appear exactly once.", nameof(endingPriority));
            Patients = profiles.AsReadOnly(); Days = calendar.AsReadOnly(); EndingPriority = priority.AsReadOnly();
            Fingerprint = ComputeFingerprint();
        }

        internal PatientDefinition Patient(string id) => Patients.Single(p => p.Id == id);
        internal DayDefinition Day(int day) => Days[day - 1];
        internal MeetingDefinition Meeting(int day, string patientId) => Day(day).Meetings.Single(m => m.PatientId == patientId);

        private string ComputeFingerprint()
        {
            // Reaction maps are normalized, so platform dictionary ordering cannot change the hash.
            var canonical = new
            {
                RulesId, Patients,
                Days = Days.Select(d => new
                {
                    d.Day, d.PurificationQuota,
                    Meetings = d.Meetings.Select(m => new
                    {
                        m.PatientId, m.NormalRounds, m.SpecialRound, m.SpecialTrustThreshold,
                        m.ReleaseTrustThreshold,
                        Reactions = m.ReactionNodes.OrderBy(p => p.Key).Select(p => new { p.Key, p.Value })
                    })
                }),
                EndingPriority
            };
            using var hash = SHA256.Create();
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(SnapshotCodec.Canonical(canonical)));
            return string.Concat(bytes.Select(b => b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture)));
        }
    }
}
