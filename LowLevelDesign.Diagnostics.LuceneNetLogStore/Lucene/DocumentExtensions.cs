using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene
{
    internal static class DocumentExtensions
    {
        public static void AddSearchableStringField(this Document doc, String fieldName, String value) {
            if (value != null) {
                doc.Add(new Field(fieldName, value, Field.Store.YES, Field.Index.ANALYZED));
            }
        }

        public static void AddSearchableDateTimeField(this Document doc, String fieldName, DateTime value) {
            if (value != default(DateTime)) {
                doc.Add(new Field(fieldName, DateTools.DateToString(value, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.ANALYZED));
            }
        }

        public static void AddSearchableNumericField(this Document doc, String fieldName, int value) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(value));
        }

        public static void AddSearchableNumericField(this Document doc, String fieldName, float value) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, true).SetFloatValue(value));
        }

        public static void AddSearchableNumericField(this Document doc, String fieldName, double value) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, true).SetDoubleValue(value));
        }
    }
}
