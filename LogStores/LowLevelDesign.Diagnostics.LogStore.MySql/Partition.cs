using System;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public class Partition : IComparable<Partition>
    {
        public const String PartitionPrefix = "olderthan";

        public String Name { get; set; }

        public DateTime GetEndDate() {
            if (!Name.StartsWith(PartitionPrefix, StringComparison.OrdinalIgnoreCase)) {
                throw new Exception("Invalid partition name.");
            }
            return DateTime.ParseExact(Name.Substring(PartitionPrefix.Length), "yyyyMMdd", null);
        }

        public static Partition ForWeek(DateTime dateInWeek) {
            dateInWeek = dateInWeek.Date;
            if (dateInWeek.DayOfWeek != DayOfWeek.Sunday) {
                // look for past sunday
                dateInWeek = dateInWeek.AddDays(-(int)dateInWeek.DayOfWeek);
            }

            return new Partition {
                Name = string.Format("{0}{1:yyyyMMdd}", PartitionPrefix, dateInWeek),
            };
        }

        public static Partition ForDay(DateTime day) {
            return new Partition {
                Name = string.Format("{0}{1:yyyyMMdd}", PartitionPrefix, day.AddDays(1)),
            };
        }

        public static Partition ForMonth(DateTime date) {
            date = new DateTime(date.Year, date.Month, 1);
            return new Partition {
                Name = String.Format("{0}{1:yyyyMM}", PartitionPrefix, date.AddMonths(1)),
            };
        }


        public int CompareTo(Partition other) {
            return String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            var other = obj as Partition;
            if (other == null) {
                return false;
            }
            return String.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() {
            if (Name == null) {
                return 0;
            }
            return Name.GetHashCode();
        }
    }
}
