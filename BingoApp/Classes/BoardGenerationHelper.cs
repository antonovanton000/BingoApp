using BingoApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Classes
{
    public class BoardGenerationHelper
    {
        public static string Generate(BoardPreset boardPreset)
        {
            var ram = new Random();
            var squares = boardPreset.Squares.OrderBy(i => ram.NextDouble()).Take(25).ToList();
            var res = CheckAndReplaceExceptions(squares, boardPreset);
            Debug.WriteLine($"CheckAndReplaceExceptions: {res}");
            while (!res)
            {                
                res = CheckAndReplaceExceptions(squares, boardPreset);
                Debug.WriteLine($"CheckAndReplaceExceptions: {res}");
            }
            var json = JsonConvert.SerializeObject(squares);
            return json;
        }

        private static bool CheckAndReplaceExceptions(List<PresetSquare> squares, BoardPreset preset)
        {
            var ram = new Random();
            foreach (var exception in preset.Exceptions)
            {
                var matched = squares.Where(i => exception.SquareNames.Any(j => j == i.Name));
                Debug.WriteLine($"matchedCount: {matched.Count()}");
                if (matched.Count() > 1)
                {
                    var sortMatched = matched.OrderBy(i => ram.NextDouble()).ToArray();
                    for (var j = 1; j < sortMatched.Count(); j++)
                    {
                        var square = squares.FirstOrDefault(i => i.Name == sortMatched[j].Name);
                        if (square != null)
                        {
                            var squareIndex = squares.IndexOf(square);
                            var newRandomSquare = GetRandomSquare(preset, exception, squares);
                            Debug.WriteLine($"Replace Square: {square.Name} -> {newRandomSquare.Name}");
                            squares[squareIndex] = newRandomSquare;
                        }
                    }
                }
            }

            foreach (var exception in preset.Exceptions)
            {
                var matched = squares.Where(i => exception.SquareNames.Any(j => j == i.Name));
                if (matched.Count() > 1)
                {
                    return false;
                }
            }
            return true;
        }

        private static PresetSquare GetRandomSquare(BoardPreset preset, PresetSquareException exception, List<PresetSquare> squares)
        {
            var allGood = preset.Squares.Where(i => !exception.SquareNames.Any(j => j.ToLower() == i.Name.ToLower()) && !squares.Any(j => j.Name.ToLower() == i.Name.ToLower()));
            var ram = new Random();
            var randomSquare = allGood.OrderBy(i => ram.NextDouble()).First();
            return randomSquare;
        }
    }
}

// 1 - получить 25 квадратиков
// 2 - взять один квадратик и искасть все исключения в которых он есть
// 3 - пройтись по всем квадратикам в исключении и если он есть на доске заменить на другой рандомный
// 4 - 
