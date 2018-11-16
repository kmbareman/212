using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems) 
		        {
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {               
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ",name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges) {
                    Console.Write("{0} ",e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);     
        }

        // List all the people who have no parents
        private static void ShowOrphans()
        {
            foreach (GraphNode n in rg.nodes)
            {
                List<GraphEdge> parentEdges = n.GetEdges("hasParent");
                if (parentEdges != null)
                {
                    foreach (GraphEdge e in parentEdges)
                    {
                        Console.Write("{0}, ", e.To());
                    }
                }
            }


            //for all names
            //  if name hasParent
            //      do nothing
            //  if name does not have hasParent
            //      add name to list of orphans
        }

        // List the shortest path between two people
        private static void ShowBingo(string name1, string name2)
        {
            //go to node name1
            //while node != name2
            //  go to all the nodes from name1
            //      from these nodes go to all of their next nodes
            //if node == name2
            //  follow path between name1 and name2

            GraphNode n1 = rg.GetNode(name1);
            GraphNode n2 = rg.GetNode(name2);
            while (n1 != n2)
            {
                //Console.Write("Relationship between {0} and {1}.", n1, n2);
                //foreach (GraphEdge e in rg.nodes)
                //{
                //    List<GraphEdge> parentEdges = n.GetEdges("hasParent");
                //}
            }
        }
        
        // List all the descendents of a person
        private static void ShowDescendents(string name)
        {
            //do a BFS starting at name to find children, then grandchildren, then great grandchildren...
            //keep counter to tell children, grandchildren... apart
            //GetNode(name) and add to Queue labeled as 0
            //  add every hasChild node to Queue and label as 1
            //  from those nodes
            //      add every hasChild node to Queue and label as 2
            //      from that list
            //          add every hasChild node to Queue and label as 3...
            //remove nodes from Queue
            //if node == 0: don't print
            //if node == 1: label as child
            //if node == 2: label as grandchild
            //if node >= 3: label as (node - 2)* great grandchild
            GraphNode n = rg.GetNode(name);
            Queue<String> descendent = new Queue<String>();
            //int counter = 0;
            descendent.Enqueue(name);
            if (n != null)
            {
                Console.Write("{0}'s descendents: ", name);
                //Queue<GraphEdge> childEdge = descendent.GetEdges("hasChild");
                List<GraphEdge> childEdges = n.GetEdges("hasChild");        //hasParent
                foreach (GraphEdge e in childEdges)
                {
                    Console.Write("{0} ", e.To());                          //e.From()
                    ShowDescendents(e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }
        
        // List all of the person's nth-cousins, k times removed
        private static void CousinsNK(string name, int n, int k)
        {
            //go up hasParent n+1 times for all unexplored paths
            // from those nodes, go down hasChild n+k+1 times for all paths that are unexplored
            //  add all of these names to list of nk cousins
            //go up hasParent n+k+1 times for all unexplored paths
            // from those nodes, go down hasChild n+1 times for all paths that are unexplored
            //  add all of these names to list of nk cousins
            //print list of nk cousins
            GraphNode r = rg.GetNode(name);
            if (r != null)
            {
                Console.Write("{0}'s {1} cousins, {2} times removed:\n", name, n, k);
                
            }
        }

        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Harry's Dutch Bingo Parlor!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    ;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                else if (command == "orphans")
                    ShowOrphans();

                else if (command == "bingo")
                    ShowBingo(commandWords[1], commandWords[2]);

                else if (command == "descendants")
                    ShowDescendents(commandWords[1]);

                else if (command == "cousins")
                    CousinsNK(commandWords[1], Convert.ToInt32(commandWords[2]), Convert.ToInt32(commandWords[3]));

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [personname], " +
                        "orphans, bingo [personname] [personname], cousins [personname] [nth cousins] [k times removed], exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}
