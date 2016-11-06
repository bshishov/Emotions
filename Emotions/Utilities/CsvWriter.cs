using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Emotions.Utilities
{
    public class CsvWriter : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly FieldInfo[] _fields;

        public CsvWriter(string path, params string[] header)
        {
            _writer = new StreamWriter(path);


            if (header.Length > 0)
            {
                _writer.Write(String.Join(",", header) + "\n");
            }
        }

        public CsvWriter(string path, Type type)
        {
            _writer = new StreamWriter(path);

            _fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var fieldNames = _fields.Select(p => p.Name).ToArray();
            if (fieldNames.Length > 0)
            {
                _writer.Write(String.Join(",", fieldNames) + "\n");
            }
        }

        public void Write(params object[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException("No values");

            var strings = new List<string>();
            foreach (var value in values)
            {
                if (value == null)
                    strings.Add(string.Empty);
                else if (value is string)
                    strings.Add(S((string)value));
                else if (value is double)
                    strings.Add(S((double)value));
                else if (value is float)
                    strings.Add(S((float)value));
                else if (value is int)
                    strings.Add(S((int)value));
                else if (value is long)
                    strings.Add(S((long)value));
                else
                    strings.Add(S(value.ToString()));
            }

            _writer.Write(String.Join(",", strings) + "\n");
        }

        public void WriteObject(object obj)
        {
            Write(_fields.Select(p => p.GetValue(obj)).ToArray());
        }

        private string S(int val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        private string S(long val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        private string S(double val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        private string S(float val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        private string S(string s)
        {
            return $"\"{s}\"";
        }

        public void Dispose()
        {
            _writer.Close();
            _writer.Dispose();
        }
    }
}
