using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Search
{
    public static class KeywordsParser
    {
        class SearchKeywordResult
        {
            public static readonly SearchKeywordResult NotFound = new SearchKeywordResult { Start = -1, End = -1 };

            public int Start { get; set; }

            public int End { get; set; }

            public string Keyword { get; set; }

            public string KeywordValue { get; set; }
        }

        public static KeywordsParsed Parse(string txt)
        {
            var result = new KeywordsParsed();

            if (txt != null) {
                var foundKeywords = new Dictionary<string, string>() {
                    { "http", null }, { "ip", null },
                    { "url", null }, { "service", null }
                };

                foreach (var keyword in foundKeywords.Keys.ToArray())
                {
                    var keywordSearchResult = FindAndParseKeyword(keyword, txt);
                    if (keywordSearchResult != SearchKeywordResult.NotFound)
                    {
                        foundKeywords[keyword] = keywordSearchResult.KeywordValue;
                        txt = ReplaceFoundKeywordWithSpace(keywordSearchResult, txt);
                    }
                }

                result.FreeText = txt;
                result.ClientIp = foundKeywords["ip"];
                result.HttpStatus = foundKeywords["http"];
                result.Service = foundKeywords["service"];
                result.Url = foundKeywords["url"];
            }
            return result;
        }

        private static string ReplaceFoundKeywordWithSpace(SearchKeywordResult keywordSearchResult, string txt)
        {
            var buffer = new StringBuilder(txt);
            buffer = buffer.Remove(keywordSearchResult.Start, keywordSearchResult.End - keywordSearchResult.Start);
            if (keywordSearchResult.Start > 0) {
                buffer.Insert(keywordSearchResult.Start, ' ');
            } else {
                buffer.Remove(keywordSearchResult.Start, 1);
            }
            return buffer.ToString();
        }

        private static SearchKeywordResult FindAndParseKeyword(string keyword, string txt)
        {
            string keywordWithColon = keyword + ":";
            int potentialKeywordPosition = txt.IndexOf(keywordWithColon);
            while (potentialKeywordPosition >= 0) {
                if (potentialKeywordPosition + keywordWithColon.Length == txt.Length) {
                    return SearchKeywordResult.NotFound;
                }
                if (potentialKeywordPosition == 0 || Char.IsWhiteSpace(txt[potentialKeywordPosition - 1])) {
                    int potentialKeywordValuePosition = potentialKeywordPosition + keywordWithColon.Length;
                    if (txt[potentialKeywordValuePosition] == '\'') {
                        if (potentialKeywordValuePosition + 1 == txt.Length) {
                            return SearchKeywordResult.NotFound;
                        }
                        int keywordValueEndPosition = txt.IndexOf('\'', potentialKeywordValuePosition + 1);
                        if (keywordValueEndPosition >= 0) {
                            return new SearchKeywordResult {
                                Keyword = keyword,
                                KeywordValue = txt.Substring(potentialKeywordValuePosition+ 1, 
                                                    keywordValueEndPosition - potentialKeywordValuePosition - 1),
                                Start = Math.Max(0, potentialKeywordPosition - 1),
                                End = Math.Min(keywordValueEndPosition + 1, txt.Length - 1)
                            };
                        }
                    } else if (!Char.IsWhiteSpace(txt[potentialKeywordValuePosition])) {
                        int keywordValueEndPosition = FindNextWhitespacePosition(txt, potentialKeywordValuePosition);
                        if (keywordValueEndPosition >= 0) {
                            return new SearchKeywordResult {
                                Keyword = keyword,
                                KeywordValue = txt.Substring(potentialKeywordValuePosition, 
                                                    keywordValueEndPosition - potentialKeywordValuePosition),
                                Start = Math.Max(0, potentialKeywordPosition - 1),
                                End = Math.Min(keywordValueEndPosition, txt.Length - 1)
                            };
                        }
                    }
                }

                potentialKeywordPosition = txt.IndexOf(keywordWithColon, potentialKeywordPosition + 1);
            }
            return SearchKeywordResult.NotFound;
        }

        private static int FindNextWhitespacePosition(string txt, int startPosition)
        {
            for (int i = startPosition; i < txt.Length; i++) {
                if (Char.IsWhiteSpace(txt[i])) {
                    return i;
                }
            }
            return -1;
        }
    }
}
