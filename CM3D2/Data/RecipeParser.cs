using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EffekseerPlayer.CM3D2.Data {
    public class RecipeParser {
        private static readonly RecipeParser INSTANCE = new RecipeParser();
        public static RecipeParser Instance {
            get {return INSTANCE; }
        }

        internal const char ESCAPE_CHAR = '\\';

        public RecipeSet ParseSet(StreamReader reader) {
            string key = null;
            string buff = null;

            var depth = -1;
            var startArray = false;
            var listValueToken = false;
            var startQuote = -1;
            var recipeSet = new RecipeSet();

            var builder = new StringBuilder();
            while (!reader.EndOfStream) {
                var quoted = false;
                var startIdx = 0;
                var line = reader.ReadLine();
                if (line == null) continue;
                line = line.Trim();
                for (var i=0; i<line.Length; i++) {
                    var chr = line[i];
                    if (quoted && chr == '"') {
                        // エスケープ判定
                        if (line[i - 1] != ESCAPE_CHAR) {
                            quoted = false;
                            if (depth == 0) {
                                buff = line.Substring(startQuote, i-startQuote);
                                continue;
                            }
                        }
                    }
                    if (quoted) continue;

                    if (chr == '{') {
                        depth++;
                        if (depth == 0){
                            startIdx = i+1;
                            continue;
                        }
                    } else if (chr == '}') {
                        depth--;
                        if (listValueToken && startArray && depth == 0) {
                            builder.Append(chr);
                            var recipe = JsonUtility.FromJson<PlayRecipe>(builder.ToString());
                            Log.Debug("recipe name:", recipe.name, ", slotID:", recipe.attachSlot, ", bone:", recipe.attachBone);
                            recipeSet.recipeList.Add(recipe);
                            builder.Length = 0;
                            continue;
                        }
                    }
                    if (listValueToken && depth > 0) {
                        builder.Append(chr);
                        continue;
                    }

                    switch (chr) {
                    case '[':
                        startArray = true;
                        break;
                    case ']':
                        startArray = false;
                        break;
                    case ':':
                        key = buff;
                        buff = null;
                        startIdx = i+1;
                        if (key == "recipeList") {
                            listValueToken = true;
                        }

                        break;
                    case ',':
                        if (!startArray) {
                            var value = buff ?? line.Substring(startIdx, i-startIdx).Trim();
                            buff = null;
                            if (key == "name") {
                                recipeSet.name = value;
                                Log.Debug("set name:", value);
                            }
                            key = null;
                            listValueToken = false;
                        }
                        startIdx = i+1;
                        break;
             //       case EscapeChar:
             //           if (i+5< line.Length) {
             //               if (line[i+1] == 'u') {
             //                   int utf32;
             //                   if (Int32.TryParse(line.Substring(i+2, 4),
									    //NumberStyles.AllowHexSpecifier,
									    //NumberFormatInfo.InvariantInfo,
             //                           out utf32)) {
             //                       builder.Append(Char.ConvertFromUtf32(utf32));
             //                       i += 5;
             //                       continue;
             //                   }

             //               }
             //           }
             //           break;
                    case '"':
                        quoted = true;
                        startQuote = i+1;
                        break;
                    }
                }
            }
            return recipeSet;
        }
    }
}
