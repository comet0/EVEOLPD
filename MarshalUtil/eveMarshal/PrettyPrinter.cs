﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace eveMarshal
{

    public static class PrettyPrinter
    {
        public const string Indention = "    ";

        public static bool IsASCII(this string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        public static string Print(PyObject obj, int indention = 0)
        {
            var ret = new StringBuilder();
            Print(ret, indention, obj);
            return ret.ToString();
        }

        private static void Print(StringBuilder builder, int indention, PyObject obj)
        {
            var indent = "";
            for (int i = 0; i < indention; i++)
                indent += Indention;

            if (obj is PyString)
                builder.AppendLine(PrintString(obj as PyString, indent, indention) + PrintRawData(obj));
            else if (obj is PyNone)
                builder.AppendLine(indent + PrintNone(obj as PyNone) + PrintRawData(obj));
            else if (obj is PyFloat)
                builder.AppendLine(indent + PrintFloat(obj as PyFloat) + PrintRawData(obj));
            else if (obj is PyInt)
                builder.AppendLine(indent + PrintInt(obj as PyInt) + PrintRawData(obj));
            else if (obj is PyIntegerVar)
                builder.AppendLine(indent + PrintIntegerVar(obj as PyIntegerVar) + PrintRawData(obj));
            else if (obj is PyTuple)
            {
                var tuple = obj as PyTuple;
                builder.AppendLine(indent + PrintTuple(tuple) + PrintRawData(obj));
                foreach (var item in tuple.Items)
                    Print(builder, indention + 1, item);
            }
            else if (obj is PyList)
            {
                var list = obj as PyList;
                builder.AppendLine(indent + PrintList(list) + PrintRawData(obj));
                foreach (var item in list.Items)
                    Print(builder, indention + 1, item);
            }
            else if (obj is PyLongLong)
                builder.AppendLine(indent + PrintLongLong(obj as PyLongLong) + PrintRawData(obj));
            else if (obj is PyBuffer)
                builder.AppendLine(indent + PrintBuffer(obj as PyBuffer) + PrintRawData(obj));
            else if (obj is PyObjectData)
            {
                var objdata = obj as PyObjectData;
                builder.AppendLine(indent + PrintObjectData(objdata) + PrintRawData(obj));
                Print(builder, indention + 1, objdata.Arguments);
            }
            else if (obj is PySubStream)
            {
                var sub = obj as PySubStream;
                builder.AppendLine(indent + PrintSubStream(sub) + PrintRawData(obj));
                Print(builder, indention + 1, sub.Data);
            }
            else if (obj is PyDict)
            {
                var dict = obj as PyDict;
                builder.AppendLine(indent + PrintDict(dict) + PrintRawData(obj));
                foreach (var kvp in dict.Dictionary)
                {
                    Print(builder, indention + 1, kvp.Key);
                    Print(builder, indention + 1, kvp.Value);
                }
            }
            else if (obj is PyObjectEx)
            {
                var objex = obj as PyObjectEx;
                builder.AppendLine(indent + PrintObjectEx(objex) + PrintRawData(obj));
                Print(builder, indention + 1, objex.Header);
                foreach (var item in objex.List)
                    Print(builder, indention + 1, item);
                foreach (var kvp in objex.Dictionary)
                {
                    Print(builder, indention + 1, kvp.Key);
                    Print(builder, indention + 1, kvp.Value);
                }
            }
            else if (obj is PyToken)
            {
                builder.AppendLine(indent + PrintToken(obj as PyToken) + PrintRawData(obj));
            }
            else if (obj is PyPackedRow)
            {
                var packedRow = obj as PyPackedRow;
                builder.AppendLine(indent + PrintPackedRow(packedRow));
                if (packedRow.Columns != null)
                {
                    foreach (var column in packedRow.Columns)
                    {
                        builder.AppendLine(indent + Indention + "[\"" + column.Name + "\" => " + column.Value +
                                           " [" + column.Type + "]]");
                    }
                }
                else
                    builder.AppendLine(indent + Indention + "[Columns parsing failed!]");
            }
            else if (obj is PyBool)
            {
                builder.AppendLine(indent + PrintBool(obj as PyBool) + PrintRawData(obj));
            }
            else if (obj is PySubStruct)
            {
                var subs = obj as PySubStruct;
                builder.AppendLine(indent + PrintSubStruct(subs) + PrintRawData(obj));
                Print(builder, indention + 1, subs.Definition);
            }
            else if (obj is PyChecksumedStream)
            {
                var chk = obj as PyChecksumedStream;
                builder.AppendLine(indent + PrintChecksumedStream(chk));
                Print(builder, indention + 1, chk.Data);
            }
            else
                builder.AppendLine(indent + "[Warning: unable to print " + obj.Type + "]");
        }

        private static string PrintChecksumedStream(PyChecksumedStream obj)
        {
            return "[PyChecksumedStream Checksum: " + obj.Checksum + "]";
        }

        private static string PrintRawData(PyObject obj)
        {
            if (obj.RawSource == null)
                return "";
            return " [" + BitConverter.ToString(obj.RawSource, 0, obj.RawSource.Length > 8 ? 8 : obj.RawSource.Length) + "]";
        }

        private static string PrintSubStruct(PySubStruct substruct)
        {
            return "[PySubStruct]";
        }

        private static string PrintBool(PyBool boolean)
        {
            return "[PyBool " + boolean.Value + "]";
        }

        private static string PrintPackedRow(PyPackedRow packedRow)
        {
            return "[PyPackedRow " + packedRow.RawData.Length + " bytes]";
        }

        private static string PrintToken(PyToken token)
        {
            return "[PyToken " + token.Token + "]";
        }

        private static string PrintObjectEx(PyObjectEx obj)
        {
            return "[PyObjectEx " + (obj.IsType2 ? "Type2" : "Normal") + "]";
        }

        private static string PrintDict(PyDict dict)
        {
            return "[PyDict " + dict.Dictionary.Count + " kvp]";
        }

        private static string PrintSubStream(PySubStream sub)
        {
            if (sub.RawData != null)
                return "[PySubStream " + sub.RawData.Length + " bytes]";
            return "[PySubStream]";
        }

        private static string PrintIntegerVar(PyIntegerVar intvar)
        {
            return "[PyIntegerVar " + intvar.IntValue + "]";
        }

        private static string PrintList(PyList list)
        {
            return "[PyList " + list.Items.Count + " items]";
        }

        private static string PrintObjectData(PyObjectData data)
        {
            return "[PyObjectData Name: " + data.Name + "]";
        }

        private static string PrintBuffer(PyBuffer buf)
        {
            return "[PyBuffer " + buf.Data.Length + " bytes]";
        }

        private static string PrintLongLong(PyLongLong ll)
        {
            return "[PyLongLong " + ll.Value + "]";
        }

        private static string PrintTuple(PyTuple tuple)
        {
            return "[PyTuple " + tuple.Items.Count + " items]";
        }

        private static string PrintInt(PyInt integer)
        {
            return "[PyInt " + integer.Value + "]";
        }

        private static string PrintFloat(PyFloat fl)
        {
            return "[PyFloat " + fl.Value + "]";
        }

        public static string StringToHex(string str)
        {
            return BitConverter.ToString(Encoding.Default.GetBytes(str)).Replace("-", "");
        }

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        private static string PrintString(PyString str, string indention, int indent)
        {
            if (str.Raw.Length > 0 && str.Raw[0] == (byte)120)
            {
                // We have serialized python data, decode and display it.
                string python = "";
                try
                {
                    Unmarshal un = new Unmarshal();
                    PyObject obj = un.Process(str.Raw);
                    python = PrettyPrinter.Print(obj, indent + 1);
                }
                catch (Exception e)
                {
                    python = "";
                }
                if (python.Length > 0)
                    return indention + "[PyString " + Environment.NewLine + python + indention + "]";
            }
            if (!containsBinary(str.Raw))
            {
                return indention + "[PyString \"" + str.Value + "\"]";
            }
            else
            {
                return indention + "[PyString \"" + str.Value + "\"" + Environment.NewLine + indention + "          <binary len=" + str.Value.Length + "> hex=\"" + ByteArrayToString(str.Raw) + "\"]";
            }
        }

        private static bool containsBinary(byte[] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] < (byte)32 || p[i] > (byte)126)
                    return true;
            }
            return false;
        }

        private static string PrintNone(PyNone none)
        {
            return "[PyNone]";
        }
    }

}
