using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                        txt = ReplaceFoundKeywordAndSpaceAroungItWithSpace(keywordSearchResult, txt);
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

        private static string ReplaceFoundKeywordAndSpaceAroungItWithSpace(SearchKeywordResult keywordSearchResult, string txt)
        {
            var buffer = new StringBuilder(txt);
            // remove space before the keyword (if exists)
            int start = keywordSearchResult.Start > 0 ? keywordSearchResult.Start - 1 : 0;
            // remove space after the keyword (if exists)
            int end = keywordSearchResult.End < txt.Length - 1 ? keywordSearchResult.End + 1 : txt.Length - 1;
            buffer = buffer.Remove(start, end - start + 1);
            if (start > 0 && buffer.Length > 0) {
                buffer.Insert(start, ' ');
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
                                Start = potentialKeywordPosition,
                                End = keywordValueEndPosition
                            };
                        }
                    } else if (!Char.IsWhiteSpace(txt[potentialKeywordValuePosition])) {
                        int keywordValueEndPosition = FindKeywordEndPosition(txt, potentialKeywordValuePosition);
                        if (keywordValueEndPosition >= 0) {
                            return new SearchKeywordResult {
                                Keyword = keyword,
                                KeywordValue = txt.Substring(potentialKeywordValuePosition, 
                                                    keywordValueEndPosition - potentialKeywordValuePosition + 1),
                                Start = potentialKeywordPosition,
                                End = keywordValueEndPosition
                            };
                        }
                    }
                }

                potentialKeywordPosition = txt.IndexOf(keywordWithColon, potentialKeywordPosition + 1);
            }
            return SearchKeywordResult.NotFound;
        }

        private static int FindKeywordEndPosition(string txt, int startPosition)
        {
            for (int i = startPosition; i < txt.Length; i++) {
                if (Char.IsWhiteSpace(txt[i])) {
                    return i - 1;
                }
            }
            return txt.Length - 1;
        }
    }
}
