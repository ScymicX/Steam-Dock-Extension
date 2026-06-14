namespace SteamDockExtension.Services;

internal sealed class VdfObject
{
    private readonly Dictionary<string, VdfValue> _values = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<KeyValuePair<string, VdfValue>> Values => _values;

    public void Add(string key, VdfValue value) => _values[key] = value;

    public string? GetString(string key) =>
        _values.TryGetValue(key, out var value) ? value.Text : null;

    public VdfObject? GetObject(string key) =>
        _values.TryGetValue(key, out var value) ? value.Object : null;
}

internal sealed record VdfValue(string? Text, VdfObject? Object)
{
    public static VdfValue FromText(string text) => new(text, null);

    public static VdfValue FromObject(VdfObject value) => new(null, value);
}

internal static class VdfParser
{
    public static VdfObject ParseFile(string path) => Parse(File.ReadAllText(path));

    public static VdfObject Parse(string content)
    {
        var reader = new TokenReader(content);
        return ParseObject(reader, stopAtClosingBrace: false);
    }

    private static VdfObject ParseObject(TokenReader reader, bool stopAtClosingBrace)
    {
        var result = new VdfObject();

        while (reader.TryRead(out var key))
        {
            if (key == "}")
            {
                if (stopAtClosingBrace)
                {
                    return result;
                }

                throw new FormatException("Unexpected closing brace in VDF document.");
            }

            if (!reader.TryRead(out var value))
            {
                throw new FormatException($"Missing value for VDF key '{key}'.");
            }

            if (value == "{")
            {
                result.Add(key, VdfValue.FromObject(ParseObject(reader, stopAtClosingBrace: true)));
            }
            else
            {
                result.Add(key, VdfValue.FromText(value));
            }
        }

        if (stopAtClosingBrace)
        {
            throw new FormatException("Missing closing brace in VDF document.");
        }

        return result;
    }

    private sealed class TokenReader
    {
        private readonly string _content;
        private int _position;

        public TokenReader(string content)
        {
            _content = content;
        }

        public bool TryRead(out string token)
        {
            SkipTrivia();
            if (_position >= _content.Length)
            {
                token = string.Empty;
                return false;
            }

            var current = _content[_position];
            if (current is '{' or '}')
            {
                token = current.ToString();
                _position++;
                return true;
            }

            if (current == '"')
            {
                token = ReadQuotedString();
                return true;
            }

            var start = _position;
            while (_position < _content.Length &&
                   !char.IsWhiteSpace(_content[_position]) &&
                   _content[_position] is not '{' and not '}')
            {
                _position++;
            }

            token = _content[start.._position];
            return token.Length > 0;
        }

        private string ReadQuotedString()
        {
            _position++;
            var value = new System.Text.StringBuilder();

            while (_position < _content.Length)
            {
                var current = _content[_position++];
                if (current == '"')
                {
                    return value.ToString();
                }

                if (current == '\\' && _position < _content.Length)
                {
                    var escaped = _content[_position++];
                    if (escaped is '\\' or '"')
                    {
                        value.Append(escaped);
                    }
                    else
                    {
                        value.Append('\\');
                        value.Append(escaped);
                    }

                    continue;
                }

                value.Append(current);
            }

            throw new FormatException("Unterminated quoted string in VDF document.");
        }

        private void SkipTrivia()
        {
            while (_position < _content.Length)
            {
                if (char.IsWhiteSpace(_content[_position]))
                {
                    _position++;
                    continue;
                }

                if (_content[_position] == '/' &&
                    _position + 1 < _content.Length &&
                    _content[_position + 1] == '/')
                {
                    _position += 2;
                    while (_position < _content.Length && _content[_position] is not '\r' and not '\n')
                    {
                        _position++;
                    }

                    continue;
                }

                break;
            }
        }
    }
}
