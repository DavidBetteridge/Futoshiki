using System;
using System.Collections.Generic;
using System.Linq;

namespace Futoshiki
{

    interface IConstraint
    {
        bool ValueNotValid(int possibleValue, Cell[,] board);
    }

    class Cell
    {
        public List<int> PossibleValues { get; set; }

        public List<IConstraint> Constraints { get; set; }
    }

    class ColumnConstraint : IConstraint
    {
        public ColumnConstraint(int columnNumber, int rowNumber)
        {
            RowNumber = rowNumber;
            ColumnNumber = columnNumber;
        }

        public int RowNumber { get; }
        public int ColumnNumber { get; }

        public bool ValueNotValid(int possibleValue, Cell[,] board)
        {
            var allPossibleValues = new List<string>();
            for (int row = 0; row < 5; row++)
            {
                if (row != RowNumber)
                {
                    var possibleValues = board[ColumnNumber, row].PossibleValues;
                    if (possibleValues.Count == 1 && possibleValues.Single() == possibleValue)
                        return true;

                    if (possibleValues.Count == 2 && possibleValues.Contains(possibleValue))
                        allPossibleValues.Add(string.Join("", possibleValues));
                }
            }

            // Are there two pairs in this column which contain this value?
            if (allPossibleValues.Distinct().Count() != allPossibleValues.Count())
                return true;

            return false;
        }
    }

    class RowConstraint : IConstraint
    {
        public RowConstraint(int columnNumber, int rowNumber)
        {
            RowNumber = rowNumber;
            ColumnNumber = columnNumber;
        }

        public int RowNumber { get; }
        public int ColumnNumber { get; }

        public bool ValueNotValid(int possibleValue, Cell[,] board)
        {
            var allPossibleValues = new List<string>();
            for (int column = 0; column < 5; column++)
            {
                if (column != ColumnNumber)
                {
                    var possibleValues = board[column, RowNumber].PossibleValues;
                    if (possibleValues.Count == 1 && possibleValues.Single() == possibleValue)
                        return true;

                    if (possibleValues.Count == 2 && possibleValues.Contains(possibleValue))
                        allPossibleValues.Add(string.Join("", possibleValues));
                }
            }

            // Are there two pairs in this row which contain this value?
            if (allPossibleValues.Distinct().Count() != allPossibleValues.Count())
                return true;

            return false;
        }
    }


    class LessThanConstraint : IConstraint
    {
        public LessThanConstraint(int thisColumn, int thisRow, int otherColumn, int otherRow)
        {
            ThisColumn = thisColumn;
            ThisRow = thisRow;
            OtherColumn = otherColumn;
            OtherRow = otherRow;
        }

        public int ThisColumn { get; }
        public int ThisRow { get; }
        public int OtherColumn { get; }
        public int OtherRow { get; }

        public bool ValueNotValid(int possibleValue, Cell[,] board)
        {
            var largestValue = board[OtherColumn, OtherRow].PossibleValues.Max();
            return possibleValue >= largestValue;
        }
    }

    class GreaterThanConstraint : IConstraint
    {
        public GreaterThanConstraint(int thisColumn, int thisRow, int otherColumn, int otherRow)
        {
            ThisColumn = thisColumn;
            ThisRow = thisRow;
            OtherColumn = otherColumn;
            OtherRow = otherRow;
        }

        public int ThisColumn { get; }
        public int ThisRow { get; }
        public int OtherColumn { get; }
        public int OtherRow { get; }

        public bool ValueNotValid(int possibleValue, Cell[,] board)
        {
            var smallestValue = board[OtherColumn, OtherRow].PossibleValues.Min();
            return possibleValue <= smallestValue;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var board = new Cell[5, 5];

            for (int column = 0; column < 5; column++)
            {
                for (int row = 0; row < 5; row++)
                {
                    board[column, row] = new Cell();
                    board[column, row].PossibleValues = Enumerable.Range(1, 5).ToList();
                    board[column, row].Constraints = new List<IConstraint>
                    {
                        new ColumnConstraint(column, row),
                        new RowConstraint(column, row)
                    };
                }
            }
            board[0, 1].PossibleValues = new List<int> { 2 };
            AddConstraint(board, 0, 0, 1, 0);
            AddConstraint(board, 4, 0, 4, 1);
            AddConstraint(board, 0, 0, 0, 1);

            AddConstraint(board, 0, 1, 1, 1);
            AddConstraint(board, 2, 1, 2, 2);

            AddConstraint(board, 1, 2, 2, 2);
            AddConstraint(board, 3, 2, 2, 2);
            AddConstraint(board, 4, 2, 3, 2);
            AddConstraint(board, 3, 2, 3, 3);

            AddConstraint(board, 1, 3, 1, 4);
            AddConstraint(board, 3, 3, 3, 4);

            AddConstraint(board, 1, 4, 2, 4);
            AddConstraint(board, 2, 4, 2, 3);
            AddConstraint(board, 2, 4, 3, 4);

            while (!Complete(board))
            {
                for (int column = 0; column < 5; column++)
                {
                    for (int row = 0; row < 5; row++)
                    {
                        if (board[column, row].PossibleValues.Count > 1)
                        {
                            foreach (var possibleValue in board[column, row].PossibleValues.ToList())
                            {
                                if (board[column, row].Constraints.Any(c => c.ValueNotValid(possibleValue, board)))
                                {
                                    board[column, row].PossibleValues.Remove(possibleValue);
                                }
                            }
                        }
                    }
                }

                for (int column = 0; column < 5; column++)
                {
                    for (int possibleValue = 0; possibleValue < 5; possibleValue++)
                    {
                        var cellsContainingValue = new List<int>();
                        for (int row = 0; row < 5; row++)
                        {
                            if (board[column, row].PossibleValues.Contains(possibleValue))
                            {
                                cellsContainingValue.Add(row);
                            }
                        }

                        if (cellsContainingValue.Count == 1)
                        {
                            if (board[column, cellsContainingValue.Single()].PossibleValues.Count > 1)
                            {
                                board[column, cellsContainingValue.Single()].PossibleValues = new List<int> { possibleValue };
                            }
                        }

                    }
                }

                for (int row = 0; row < 5; row++)
                {
                    for (int possibleValue = 0; possibleValue < 5; possibleValue++)
                    {
                        var cellsContainingValue = new List<int>();
                        for (int column = 0; column < 5; column++)
                        {
                            if (board[column, row].PossibleValues.Contains(possibleValue))
                            {
                                cellsContainingValue.Add(column);
                            }
                        }

                        if (cellsContainingValue.Count == 1)
                        {
                            if (board[cellsContainingValue.Single(), row].PossibleValues.Count > 1)
                            {
                                board[cellsContainingValue.Single(), row].PossibleValues = new List<int> { possibleValue };
                            }
                        }

                    }
                }

                DisplayBoard(board);

            }

            Console.ReadKey();

        }

        private static bool Complete(Cell[,] board)
        {
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    if (board[column, row].PossibleValues.Count > 1)
                        return false;
                }
            }

            return true;
        }

        private static void DisplayOptions(Cell[,] board, int column, int row)
        {
            var values = string.Join(',', board[column, row].PossibleValues);
            Console.WriteLine($"Cell {column},{row} can be {values}");
        }

        private static void DisplayBoard(Cell[,] board)
        {
            Console.WriteLine("");

            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    if (board[column, row].PossibleValues.Count > 1)
                    {
                        Console.Write("_");
                    }
                    else
                    {
                        Console.Write(board[column, row].PossibleValues.Single());
                    }
                }
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }

        private static void AddConstraint(Cell[,] board, int columnGreaterThan, int rowGreaterThan, int columnLessThan, int rowLessThan)
        {
            board[columnGreaterThan, rowGreaterThan].Constraints.Add(new GreaterThanConstraint(columnGreaterThan, rowGreaterThan, columnLessThan, rowLessThan));
            board[columnLessThan, rowLessThan].Constraints.Add(new LessThanConstraint(columnLessThan, rowLessThan, columnGreaterThan, rowGreaterThan));
        }
    }
}
