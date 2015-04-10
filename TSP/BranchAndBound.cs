using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using PriorityQueue;
using System.Diagnostics;

namespace TSP
{
    class BranchAndBound
    {
        private City[] Cities;
        private ArrayList Route;
        private TSPSolution bssf; 
        private PriorityQueue<Double,BranchAndBoundState> agenda;

        private static Timer timer;

        private static bool timeIsUp;

        public BranchAndBound( City[] cities)
        {
            this.Cities = cities;
            Route = new ArrayList();
            bssf = new TSPSolution(Route);
            agenda = new PriorityQueue<double, BranchAndBoundState>();
            timeIsUp = false;

            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(TimeIsUp);
        }

        private static void TimeIsUp(object source, ElapsedEventArgs e)
        {
            timeIsUp = true;
        }

        public TSPSolution GetBSSF() 
        {
            return bssf;
        }

        /// <summary>
        /// Greedy algorithm implementation for first bssf solution
        /// </summary>
        /// <returns></returns>
        private TSPSolution GenerateQuickSolution()
        {
            // initialize data structures
            Route = new ArrayList();
            HashSet<City> citiesLeft = new HashSet<City>(Cities);
            Route.Add(Cities[0]);
            citiesLeft.Remove(Cities[0]);
            

            while (citiesLeft.Count != 0)
            {
                City currentCity = Route[Route.Count - 1] as City;
                double bestCost = Double.PositiveInfinity;
                City bestCity = null;

                // go through cities left to find the best cost
                foreach (City city in citiesLeft)
                {
                    double cost = currentCity.costToGetTo(city);
                    if (cost < bestCost)
                    {
                        bestCity = city;
                        bestCost = cost;
                    }
                }

                // I don't think it will be null, but just incase...
                if (bestCity == null)
                {
                    throw new System.ArgumentNullException();
                }

                Route.Add(bestCity);
                citiesLeft.Remove(bestCity);


            }

            return new TSPSolution(Route);
        }

        private BranchAndBoundState init_state() {
            return null;
        }

        /// <summary>
        /// Creates cost matrix based on the cities list.  Places infinity on the diagonal too.
        /// </summary>
        /// <returns></returns>
        private Double[,] generateCostMatrix()
        {
            Double[,] matrix = new Double[Cities.Length,Cities.Length];

            for (int from_index = 0; from_index < Cities.Length; from_index++)
            {
                for (int to_index = 0; to_index < Cities.Length; to_index++) 
                {
                    City from = Cities[from_index];
                    City to = Cities[to_index];

                    if( to_index == from_index)
                    {
                        matrix[from_index, to_index] = Double.PositiveInfinity;
                    }
                    else
                    {
                        matrix[from_index, to_index] = from.costToGetTo(to);
                    }
                }
            }

            return matrix;
        }
        
        public TSPSolution AnalyzePath( int max_running_time, bool useBaseAlgorithm = false ) {

            int maxAgendaSize = 0;

            // start timer
            timer.Interval = max_running_time * 1000;
            timer.Start();
            DateTime startTime = DateTime.Now;

            if (useBaseAlgorithm)
            {
                bssf = GenerateQuickSolution();

                // end timer and display the results
                DateTime quickEndTime = DateTime.Now;
                Program.MainForm.tbElapsedTime.Text = " " + (quickEndTime - startTime);
                
                Debug.WriteLine("Resulting JSON:");
                Debug.WriteLine(bssf.ToString());

                return bssf;
            }
	    
            // get initial state - creating the costmatrix with the given cities
            BranchAndBoundState initial_state = new BranchAndBoundState( generateCostMatrix(), new List<int>());

            // reduce the matrix
            initial_state.Reduce();

            // create bssf using the greedy algorithm
            bssf = GenerateQuickSolution();
            
            // add initial state to the queue
            agenda.Clear();
            agenda.Enqueue( initial_state.GetBound() / initial_state.getDepthLevel(), initial_state );

            while (!agenda.IsEmpty && !timeIsUp && bssf.GetCost() != agenda.Peek().Value.GetBound())
            {
                // checking size of agenda to find the max size
                if (agenda.Count > maxAgendaSize)
                {
                    maxAgendaSize = agenda.Count;
                }

                BranchAndBoundState u = agenda.Dequeue().Value;

                Debug.WriteLine("Exploring at Level: " + u.getDepthLevel());

                // check to see if current state is worth exploring (i.e. it could be better than the bssf)
                if( u.GetBound() < bssf.GetCost()) {

                    // generate successors
                    ArrayList successors = u.GetSuccessors();

                    // iterate through successors and either add them to queue, throw them away, or replace the bssf with them
                    foreach( BranchAndBoundState w in successors ) 
                    {
                        if (timeIsUp) break; 
                        
                        // check if possibly better than the bssf, otherwise prune/toss it out
                        if( w.GetBound() < bssf.GetCost())
                        {
                            // check if it has a complete path, otherwise add it to the agenda
                            if( w.IsSolution() ) 
                            {
                                // create the TSP solution
                                TSPSolution possibleBSSF = new TSPSolution(w.GetCities(Cities));

                                // check if cost of solution is better than cost of bssf.  if so, replace bssf
                                if (possibleBSSF.GetCost() < bssf.GetCost())
                                {
                                    bssf = possibleBSSF;
                                }
                                
                            }
                            else
                            {
                                agenda.Enqueue(w.GetBound() / w.getDepthLevel(), w);
                            }
                        }
                    }
                }
            }


            // end timer and display the results
            DateTime endTime = DateTime.Now;
            Program.MainForm.tbElapsedTime.Text = " " + (endTime - startTime);
            Program.MainForm.MaxAgendaSizeTextBox.Text = maxAgendaSize.ToString();

            Debug.WriteLine("Resulting JSON:");
            Debug.WriteLine(bssf.ToString());

            return bssf;

        }

    }

    /// <summary>
    /// BranchAndBoundState class holds the cost matrix, reduces the matrix, generates the successors, etc.
    /// </summary>
    public class BranchAndBoundState
    {
        Double[,] costMatrix;
        Double bound;
        List<int> Cities;
        int depthLevel;
        

        public BranchAndBoundState( Double[,] cost_matrix, List<int> cities, Double _bound = 0, int _depthLevel = 1)
        {
            costMatrix = cost_matrix;
            bound = _bound;
            Cities = cities;
            depthLevel = _depthLevel;
            
        }

        public int getDepthLevel()
        {
            return depthLevel;
        }

        /// <summary>
        /// Implementation of reduce algorithm discussed in class.  It updates the bound as well.
        /// </summary>
        public void Reduce()
        {
            ReduceRows();
            ReduceCols();
        }

        private void ReduceCols()
        {
            // reduce all cols
            for (int col = 0; col < costMatrix.GetLength(1); col++)
            {
                Double min_so_far = Double.PositiveInfinity;

                // Find minimum value in current row
                for (int row = 0; row < costMatrix.GetLength(0); row++)
                {
                    // ignore same from/to values
                    if (row != col && costMatrix[row, col] < min_so_far)
                    {
                        min_so_far = costMatrix[row, col];
                    }
                }

                // Reduce all values in this row by the minimum (unless Positive Infinity)
                if (!Double.IsPositiveInfinity(min_so_far))
                {
                    for (int row = 0; row < costMatrix.GetLength(0); row++)
                    {
                        // If we haven't already eliminated this path, reduce it
                        if (!Double.IsPositiveInfinity(costMatrix[row, col]))
                        {
                            costMatrix[row, col] = costMatrix[row, col] - min_so_far;
                        }
                    }

                    // update our bound with the reduction
                    bound += min_so_far;
                }
            }
        }

        private void ReduceRows()
        {
            // reduce all rows
            for (int row = 0; row < costMatrix.GetLength(0); row++)
            {
                Double min_so_far = Double.PositiveInfinity;

                // Find minimum value in current row
                for (int col = 0; col < costMatrix.GetLength(1); col++)
                {
                    // ignore same from/to values
                    if (col != row && costMatrix[row, col] < min_so_far)
                    {
                        min_so_far = costMatrix[row, col];
                    }
                }

                // Reduce all values in this row by the minimum (unless Positive Infinity)
                if (!Double.IsPositiveInfinity(min_so_far))
                {
                    for (int col = 0; col < costMatrix.GetLength(1); col++)
                    {
                        // If we haven't already eliminated this path, reduce it
                        if (! Double.IsPositiveInfinity(costMatrix[row, col]))
                        {
                            costMatrix[row, col] = costMatrix[row, col] - min_so_far;
                        }
                    }

                    // update our bound with the reduction
                    bound += min_so_far;
                }
            }
        }
        
        public Double GetBound() {
            return bound;
        }

        
        /// <summary>
        /// Generates the successors from the costMatrix.
        /// Uses the current path of cities to figure out the next city to explore
        /// Returns all of the possible successors.
        /// </summary>
        /// <returns></returns>
        public ArrayList GetSuccessors() {
                       
            // if we have no starting point, add the first city to the list
            if (Cities.Count == 0)
            {
                Cities.Add(0);
            }

            ArrayList successors = new ArrayList();

            int latestCity = Cities[Cities.Count - 1];

            for (int i = 0; i < costMatrix.GetLength(0); i++)
            {
                if (costMatrix[latestCity, i] != Double.PositiveInfinity && latestCity != i)
                {
                    List<int> newCities = new List<int>(Cities);
                    newCities.Add(i);
                    successors.Add( generateBranchAndBoundState(costMatrix, newCities, this.bound + costMatrix[latestCity, i], this.depthLevel + 1) );
                }
            }
            
            return successors;
        }

        /// <summary>
        /// Takes in matrix (making a copy in the process) and sets the appropriate rows and cols to be infinity.
        /// Also takes care of partial criterion (A -> D, but D cannot go to A)
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private BranchAndBoundState generateBranchAndBoundState(double[,] matrix, List<int> cities, Double lowerBound, int depthLevel)
        {
            // building copy
            Double[,] newMatrix = new Double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    newMatrix[i, j] = matrix[i, j];
                }
            }

            int from = cities[cities.Count - 2];
            int to = cities[cities.Count - 1];

            // setting row and col to be infinity
            for (int i = 0; i < newMatrix.GetLength(0); i++)
            {
                newMatrix[from, i] = Double.PositiveInfinity;
                newMatrix[i, to] = Double.PositiveInfinity;
            }

            // partial criterion:  if A -> B -> C, then C !-> A OR C !-> B
            for (int i = 0; i < cities.Count; i++)
            {
                newMatrix[to, cities[i]] = Double.PositiveInfinity;
            }

            BranchAndBoundState result = new BranchAndBoundState(newMatrix, cities, lowerBound, depthLevel);
            result.Reduce();
            return result;
        }

        // aka Criterion
        public bool IsSolution() {
            //return false;

            // would this be the right implementation of IsSolution?  Basically asking if our cities
            // array is full?
            // df - I think that'll work.
            return (Cities.Count == costMatrix.GetLength(0));
            
        }

        
        public ArrayList GetCities(City[] masterCities)
        {
            ArrayList result = new ArrayList();

            for (int i = 0; i < Cities.Count; i++)
            {
                result.Add(masterCities[Cities[i]]);
            }

            return result;
        }

        // helpful for debugging
        public void printMatrix()
        {
            for (int i = 0; i < costMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < costMatrix.GetLength(1); j++)
                {
                    Double cost = costMatrix[i, j];
                    if (cost == Double.PositiveInfinity)
                    {
                        cost = 999;
                    }
                    Debug.Write(cost + "\t");

                }

                Debug.WriteLine("");
            }
        }

    }
    
    // Base code for this class provided with CS312 TSP project
    public class TSPSolution
    {
        /// <summary>
        /// we use the representation [cityB,cityA,cityC] 
        /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
        /// and the edge from cityC to cityB is the final edge in the path.  
        /// you are, of course, free to use a different representation if it would be more convenient or efficient 
        /// for your node data structure and search algorithm. 
        /// </summary>
        public ArrayList Route;
        private Double cost;

        public TSPSolution(ArrayList iroute)// : base(null, new List<int>(), ref new City[0])
        {
            Route = new ArrayList(iroute);
            cost = GetCost(); 
        }


        /// <summary>
        ///  compute the cost of the current route.  does not check that the route is complete, btw.
        /// assumes that the route passes from the last city back to the first city. 
        /// </summary>
        /// <returns></returns>
        public double GetCost()
        {
            // If we've already done the work, just return it
            //if( this.cost > 0 )
            //    return this.cost;

            if (Route.Count == 0)
                return 0;

            // go through each edge in the route and add up the cost. 
            int x;
            City here; 
            double cost = 0D;
                
            for (x = 0; x < Route.Count-1; x++)
            {
                here = Route[x] as City;
                cost += here.costToGetTo(Route[x + 1] as City);
            }
            // go from the last city to the first. 
            here = Route[Route.Count - 1] as City;
            cost += here.costToGetTo(Route[0] as City);
            return cost; 
        }

        
        public String ToString()
        {

            StringBuilder builder = new StringBuilder();



            builder.Append('[');

            foreach (City c in Route)
            {

                builder.Append("[" + c.X + "," + c.Y + "],");

            }

            builder.Remove(builder.Length - 1, 1); //Remove extra comma at end

            builder.Append(']');



            return builder.ToString();

        }
    }
}
