using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iceland_Test
{
    class Program
    {
        /// <summary>
        /// Are we debugging the application? Will display stack traces in the Console Window
        /// </summary>
        private static Boolean _debug = true;

        static void Main(string[] args)
        {
            // Initial Setup
            // File location for items (input) for this example. Should ideally be in a DB or on API
            String itemSource = @"../../JSON/iput.json";
            // File location for allowed items within the input. Once again should be in a DB or on API
            String allowedItemSource = @"../../JSON/allowed.json";

            // Ok, we're setup, lets start actual execution
            try
            {
                // Get input from source
                List<Item> input = JsonConvert.DeserializeObject<List<Item>>(FetchInput(itemSource));

                List<String> allowedItems = JsonConvert.DeserializeObject<List<String>>(FetchInput(allowedItemSource));

                // Init the output
                List<Item> output = new List<Item>();

                // Loop through input and update for output
                Console.WriteLine();
                Console.WriteLine("Test Input :");
                foreach (Item item in input)
                {
                    Console.WriteLine("{0} {1} {2}", item.name, item.sellin, item.quality);
                    // Check that the item is allowed
                    if (!allowedItems.Exists(e => e == item.name))
                    {
                        // it's not, let's change the name and values to reflect this
                        item.name = "NO SUCH ITEM";
                        item.quality = null;
                        item.sellin = null;
                    }

                    // sell by date has passed, Quality degrades twice as fast 
                    int degredation = item.degredation;
                    if (item.sellin < 0)
                    {
                        if (degredation > 0)
                        {
                            degredation = item.degredation * -2;
                        }
                        else
                        {
                            degredation = item.degredation * 2;
                        }
                    } 

                    // Switch the item as each item has a different rate of decay/rules for quality
                    switch (item.name)
                    {
                        case "Soap":
                            // Do nothing, as it's soap
                            break;
                        case "Christmas Crackers":

                            if (item.sellin <= 10 && item.sellin > 5)
                            {
                                // Getting close to christmas, quality increases by 2
                                degredation = 2;
                            }
                            else if (item.sellin <= 5 && item.sellin > 0)
                            {
                                // Close to Christmas, quality increases by 3 now
                                degredation = 3;
                            }
                            else if (item.sellin <= 0)
                            {
                                // Override default
                                item.sellin = item.sellin - 1;
                                item.quality = 0;
                                break;
                            }

                            // Default calculation
                            item.sellin = item.sellin - 1;
                            item.quality = CalculateQuality(item.quality, degredation);
                            break;
                        default:
                            // Default calculation, item does not have any special rules
                            item.sellin = item.sellin - 1;
                            item.quality = CalculateQuality(item.quality, degredation);
                            break;
                    }

                    output.Add(item);
                }

                // Output the new values
                Console.WriteLine();
                Console.WriteLine("Expected Output :");
                foreach (Item item in output)
                {
                    Console.WriteLine("{0} {1} {2}", item.name, item.sellin, item.quality);
                }
                // TODO this should really be saved to a file again or sent somewhere

            } 
            catch (IOException ex)
            {
                if (_debug)
                {
                    Console.WriteLine("IOException Thrown:: {0}", ex.Message);
                }

                // User friendly error message
                Console.WriteLine("An error occurred whilst fetching input source. Please try again later");
            }
            finally
            {
                // Finally, pause for reading
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Calucate the new quality value of an item
        /// </summary>
        /// <param name="quality">Nullable if the item is not allowed/does not exist</param>
        /// <param name="degredation">The degredation rate of the item to calculate quality</param>
        /// <returns>Nullable<int> Null on item not allowed</returns>
        static Nullable<int> CalculateQuality(Nullable<int> quality, int degredation)
        {
            if (quality >= 50)
            {
                // Cannot be greater than 50
                return 50;
            }
            else if (quality <= 0)
            {
                // Cannot be less than 0
                return 0;
            }
            else if (quality == null)
            {
                // item does not exist
                return null;
            }
            else
            {
                // actually calculate it
                return ((int)quality + degredation);
            }
        }

        /// <summary>
        /// Fetch Input from source location
        /// </summary>
        /// <param name="source">String value of file name and dir</param>
        static String FetchInput(String source)
        {
            using (StreamReader input = new StreamReader(source))
            {
                return input.ReadToEnd();
            }
        }
    }
}
