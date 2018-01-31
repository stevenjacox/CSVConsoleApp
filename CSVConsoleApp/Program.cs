//Authored by Steven Cox

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CSVConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {

            if (args.Count() != 2)
            {
                Console.WriteLine(@"example usage -- CSVConsoleApp.exe C:\PATH\csvfiletoread.csv columnnametocheckforduplicates");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("The file you specified could not be found.");
                return;
            }

            List<IEnumerable<string>> rows = new List<IEnumerable<string>>();
            try
            {
                using (TextReader tr = new StreamReader(args[0]))
                {
                    var firstRowHeaders = tr.ReadLine().Split(',');

                    if (firstRowHeaders.Distinct().Count() < firstRowHeaders.Count())
                    {
                        Console.WriteLine("Fatal error: Some of the headers have duplicate names");
                        return;
                    }

                    //I am setting this to -1 to mean "not found yet"
                    int indexofcolumn = -1;

                    //Could use a different algorithm for searching here but opting for simplicity and readability
                    //It also looks like this is what Contains does but we are using the index number for later
                    //public bool Contains(T item) {
                    //if ((Object)item == null)
                    //{
                    //    for (int i = 0; i < _size; i++)
                    //        if ((Object)_items[i] == null)
                    //            return true;
                    //    return false;
                    //}
                    //else
                    //{
                    //    EqualityComparer<T> c = EqualityComparer<T>.Default;
                    //    for (int i = 0; i < _size; i++)
                    //    {
                    //        if (c.Equals(_items[i], item)) return true;
                    //    }
                    //    return false;
                    //}

                    for (int i = 0; i < firstRowHeaders.Length; i++)
                    {
                        if (firstRowHeaders[i].ToLower().Equals(args[1].ToLower()))
                        {
                            indexofcolumn = i;
                            break;
                        }
                    }

                    //-1 means not found in this case
                    if (indexofcolumn.Equals(-1))
                    {
                        Console.WriteLine("The column you specified was not found in the header row of the file you specified.");
                        return;
                    }

                    string line;
                    string[] splitline;

                    while ((line = tr.ReadLine()) != null)
                    {
                        //We might want to remove whitespace here just in case
                        line = line.Replace(" ", string.Empty);

                        splitline = line.Split(',');

                        //Checking to see if all the columns are the same size
                        if (splitline.Count() != firstRowHeaders.Count())
                        {
                            Console.WriteLine("Malformed data: row count must match header count");
                            return;
                        }
                        rows.Add(splitline);
                    }
                    //    GroupBy uses the following code from Enumerable.cs; with create set to true;
                    //    int hashCode = InternalGetHashCode(key);
                    //    for (Grouping g = groupings[hashCode % groupings.Length]; g != null; g = g.hashNext)
                    //        if (g.hashCode == hashCode && comparer.Equals(g.key, key)) return g;
                    //    if (create)
                    //    {
                    //        if (count == groupings.Length) Resize();
                    //        int index = hashCode % groupings.Length;
                    //        Grouping g = new Grouping();
                    //        g.key = key;
                    //        g.hashCode = hashCode;
                    //        g.elements = new TElement[1];
                    //        g.hashNext = groupings[index];
                    //        groupings[index] = g;
                    //        if (lastGrouping == null)
                    //        {
                    //            g.next = g;
                    //        }
                    //        else
                    //        {
                    //            g.next = lastGrouping.next;
                    //            lastGrouping.next = g;
                    //        }
                    //        lastGrouping = g;
                    //        count++;
                    //        return g;
                    //    }
                    //    return null;
                    //}
                    var groupsByColumnIndex = rows.GroupBy(x => x.ElementAt(indexofcolumn)).ToList();

                    if (!groupsByColumnIndex.Any(x => x.Count() > 1))
                    {
                        Console.WriteLine("We didn't find duplicate entries for {0}...", args[1]);
                        Console.ReadKey();
                        return;
                    }

                    //Showing the SQL-like syntax
                    var keysforGroupsWhereDuplicatesWereFound = from g in groupsByColumnIndex
                                                                where g.Count() > 1
                                                                select g.Key;

                    HashSet<string> keys = new HashSet<string>();

                    //Forcing the key list to be concrete to avoid reevaluation of g.Count() > 1 
                    foreach (string key in keysforGroupsWhereDuplicatesWereFound)
                    {
                        keys.Add(key);
                    }

                    Console.WriteLine("We found the following lines to have duplicates for {0}...", args[1]);

                    foreach (IEnumerable<string> row in rows)
                    {
                        if (keys.Contains(row.ElementAt(indexofcolumn)))
                        {
                            Console.WriteLine(string.Join(",", row));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception occurred: {ex} Exiting...", ex.Message);

            }

        }

    }
}
