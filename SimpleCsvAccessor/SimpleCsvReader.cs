using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleCsvAccessor
{
    public class SimpleCsvReader
    {
        const char DEFAULT_DELIMITER = ',';
        const char DEFAULT_QUALIFIER = '"';

        private TextReader _src;
        private char _delimiter;
        private char _qualifier;

        public SimpleCsvReader(TextReader src, char delimiter = DEFAULT_DELIMITER, char qualifier = DEFAULT_QUALIFIER)
        {
            if (null == src)
            {
                throw new ArgumentNullException("src");
            }

            if (delimiter == qualifier)
            {
                throw new ArgumentException("The delimiter and qualifier cannot be the same character");
            }

            if (delimiter == qualifier)
            {
                throw new ArgumentException("The delimiter cannot be the same as qualifier");
            }

            _src = src;
            _delimiter = delimiter;
            _qualifier = qualifier;
        }

        public string[] ReadRecord()
        {
            var line = _src.ReadLine();
            if (null == line)
            {
                return null;
            }

            return (new CsvLineParser(line, _delimiter, _qualifier)).Parse();
        }

        public class CsvLineParser
        {
            private string _src;
            private char _delimiter;
            private char _qualifier;
            private bool _isFirstField;
            private int _curPos;

            public CsvLineParser(string src, char delimiter, char qualifier)
            {
                _src = src;
                _delimiter = delimiter;
                _qualifier = qualifier;
                _isFirstField = true;
            }

            public string[] Parse()
            {
                _curPos = 0;
                List<string> fields = new List<string>();
                string field = null;

                while(null != (field = ParseNextField()))
                {
                    _isFirstField = false;
                    fields.Add(field);
                }

                return fields.ToArray();
            }

            private string ParseNextField()
            {
                if (_curPos >= _src.Length)
                {
                    return null;
                }

                if (!_isFirstField)
                {
                    // The delimiter is the beginning character for every field except the first field, need to skip this character
                    _curPos++;
                }

                string field = null;

                if (_curPos == _src.Length)
                {
                    // If the last character is the delimiter, then there is an empty field
                    field = string.Empty;
                }
                else
                {
                    char ch = _src[_curPos];

                    if (ch == _delimiter)
                    {
                        field = string.Empty;
                    }
                    else if (ch == _qualifier)
                    {
                        field = ReadQualifiedFieldAndRemoveQualifier();
                    }
                    else
                    {
                        field = ReadCharsBeforeNextDelimiterOrEnd();
                    }
                }
                
                return field;
            }

            private string ReadQualifiedFieldAndRemoveQualifier()
            {
                _curPos++;
                int pos = _curPos;
                StringBuilder sb = new StringBuilder();

                for (; pos < _src.Length; pos++)
                {
                    char ch = _src[pos];
                    if (_qualifier == ch)
                    {
                        pos++;

                        if ((pos == _src.Length)            // Encounter the right qualifier in the end of the line
                            || (_delimiter == _src[pos]))   // or right qualifier of current field
                        {
                            break;
                        }
                        else
                        {
                            char chX = _src[pos];
                            // The next char is the same as qualifier, be treated as escaped qualifier
                            if (_qualifier == chX)
                            {
                                sb.Append(chX);
                            }
                            else
                            {
                                // Incorrect scenario. The qualifier must the last char of the line, or in front of the delimiter of current field, 
                                // so need to discard all characters between this qualifer and the delimiter or until the end line
                                while ((pos++) < _src.Length && _delimiter != _src[pos]) 
                                    ;
                                break;
                            }
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                _curPos = pos;

                return sb.ToString();
            }

            private string ReadCharsBeforeNextDelimiterOrEnd()
            {
                int prevPos = _curPos;
                int nextPos = _curPos + 1;
                while ((nextPos < _src.Length) && (_delimiter != _src[nextPos]))
                    nextPos++;

                _curPos = nextPos;

                return _src.Substring(prevPos, nextPos - prevPos);
            }
        }
    }
}
