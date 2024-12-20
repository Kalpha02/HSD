using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Tests
{
    public static class Assert
    {
        public static void IsEqual(object value, object expected)
        {
            if (!value.Equals(expected))
                throw new Exception(String.Format("Excpeted {0} but value was {1}", expected, value));
        }

        public static void IsEqualByContent(object a, object b)
        {
            if (a.GetType() != b.GetType())
                throw new Exception("Objects are not of the same type!");

            FieldInfo[] fields = a.GetType().GetFields();

            for (int i = 0; i <  fields.Length; ++i)
                if (!fields[i].GetValue(a).Equals(fields[i].GetValue(b)))
                    throw new Exception(String.Format("Fields {0} don't match! Expected {1} but value was {2}", fields[i].Name, fields[i].GetValue(a), fields[i].GetValue(b)));
        }

        public static void That(bool value)
        {
            if (!value)
                throw new Exception("Statement invalid!");
        }

        public static void True() { } // Only exists for the sake of completion

        public static void False()
        {
            throw new Exception("Asserted false");
        }
    }
}
