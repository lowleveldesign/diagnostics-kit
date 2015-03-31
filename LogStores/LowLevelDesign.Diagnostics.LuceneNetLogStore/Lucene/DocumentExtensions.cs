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
        public static void AddField(this Document doc, String fieldName, String value, bool indexed = false) {
            if (value != null) {
                doc.Add(new Field(fieldName, value, Field.Store.YES, indexed ? Field.Index.ANALYZED : Field.Index.NO));
            }
        }

        public static void AddField(this Document doc, String fieldName, DateTime value, bool indexed = false) {
            if (value != default(DateTime)) {
                doc.Add(new Field(fieldName, DateTools.DateToString(value, DateTools.Resolution.MILLISECOND), Field.Store.YES, 
                    indexed ? Field.Index.ANALYZED : Field.Index.NO));
            }
        }

        public static void AddField(this Document doc, String fieldName, int value, bool indexed = false) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, indexed).SetIntValue(value));
        }

        public static void AddField(this Document doc, String fieldName, long value, bool indexed = false) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, indexed).SetLongValue(value));
        }

        public static void AddField(this Document doc, String fieldName, float value, bool indexed = false) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, indexed).SetFloatValue(value));
        }

        public static void AddField(this Document doc, String fieldName, double value, bool indexed = false) {
            doc.Add(new NumericField(fieldName, Field.Store.YES, indexed).SetDoubleValue(value));
        }
    }
}
