using System;
using System.Collections.Generic;
using System.Linq;

namespace NodeNetwork.Utilities
{
    //Class for calculating the longest common subsequence of two lists
    //For example: the LCS of 'computer' and 'houseboat' is 'out'
    public class LongestCommonSubsequence
    {
        /// <summary>
        /// The type of change that occured.
        /// </summary>
        public enum ChangeType
        {
            Removed, Added
        }

        /// <summary>
        /// Returns the changes to be made to oldList to reach the state of newList.
        /// First all items that are in oldList but not in the LCS of oldList and newList are removed.
        /// The list is then identical to the LCS of oldList and newList.
        /// Then all items that are in newList but not in the LCS of oldList and newList are added.
        /// The list is then identical to newList.
        /// </summary>
        /// <typeparam name="T">The type of items contained in the two lists</typeparam>
        /// <param name="oldList">The first list</param>
        /// <param name="newList">The second list</param>
        /// <returns></returns>
        public static IEnumerable<(int index, T item, ChangeType change)> GetChanges<T>(IList<T> oldList, IList<T> newList)
        {
            if (oldList == null)
            {
                throw new ArgumentNullException(nameof(oldList));
            }else if (newList == null)
            {
                throw new ArgumentNullException(nameof(newList));
            }

            T[] lcs = LongestCommonSubsequence.Calculate(oldList, newList).ToArray();

            //Initial data => LCS
            int lcsCursor = lcs.Length - 1;
            for (int initialDataCursor = oldList.Count - 1; initialDataCursor >= 0; initialDataCursor--)
            {
                if (lcsCursor >= 0 && oldList[initialDataCursor].Equals(lcs[lcsCursor]))
                {
                    lcsCursor--;
                }
                else
                {
                    yield return (initialDataCursor, oldList[initialDataCursor], ChangeType.Removed);
                }
            }

            //LCS => newdata
            lcsCursor = 0;
            for (int newDataCursor = 0; newDataCursor < newList.Count; newDataCursor++)
            {
                if (lcsCursor < lcs.Length && newList[newDataCursor].Equals(lcs[lcsCursor]))
                {
                    lcsCursor++;
                }
                else
                {
                    yield return (newDataCursor, newList[newDataCursor], ChangeType.Added);
                }
            }
        }

        /// <summary>
        /// Returns the longest common subsequence of two lists
        /// For example: the LCS of 'computer' and 'houseboat' is 'out'
        /// </summary>
        /// <typeparam name="T">The type of items contained in the two lists</typeparam>
        /// <param name="seq1">The first list</param>
        /// <param name="seq2">The second list</param>
        /// <returns>An enumerable of items that are both in seq1 and seq2 and which follows the consecutive order of both lists</returns>
        public static IEnumerable<T> Calculate<T>(IList<T> seq1, IList<T> seq2)
        {
            int[,] matrix = CalculateLCSMatrix(seq1, seq2);
            return Backtrack(seq1, seq2, matrix).Reverse();
        }

        private static int[,] CalculateLCSMatrix<T>(IList<T> seq1, IList<T> seq2)
        {
            int[,] matrix = new int[seq1.Count + 1, seq2.Count + 1];
            for (int i = 1; i < matrix.GetLength(0); i++)
            {
                for (int j = 1; j < matrix.GetLength(1); j++)
                {
                    if (seq1[i - 1].Equals(seq2[j - 1]))
                    {
                        matrix[i, j] = matrix[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        matrix[i, j] = Math.Max(matrix[i - 1, j], matrix[i, j - 1]);
                    }
                }
            }
            return matrix;
        }

        private static IEnumerable<T> Backtrack<T>(IList<T> seq1, IList<T> seq2, int[,] matrix)
        {
            int i = matrix.GetLength(0) - 1;
            int j = matrix.GetLength(1) - 1;
            bool done = false;

            while (!done)
            {
                if (i > 0 && j > 0 && seq1[i - 1].Equals(seq2[j - 1]))
                {
                    yield return seq1[i - 1];
                    i -= 1;
                    j -= 1;
                }
                else if (j > 0 && (i == 0 || matrix[i, j - 1] >= matrix[i - 1, j]))
                {
                    j -= 1;
                }
                else if (i > 0 && (j == 0 || matrix[i, j - 1] < matrix[i - 1, j]))
                {
                    i -= 1;
                }
                else
                {
                    done = true;
                }
            }
        }
    }
}
