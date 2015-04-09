using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TSP
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
       {

            //Testing the successors method...looks like it is working.
           //Double[,] cm = new Double[5, 5];
           //cm[0, 0] = Double.PositiveInfinity;
           //cm[0, 1] = 2;
           //cm[0, 2] = 3;
           //cm[0, 3] = 4;
           //cm[0, 4] = 2;
           //cm[1, 0] = 0;
           //cm[1, 1] = Double.PositiveInfinity;
           //cm[1, 2] = 1;
           //cm[1, 3] = 6;
           //cm[1, 4] = 7;
           //cm[2, 0] = 8;
           //cm[2, 1] = 2;
           //cm[2, 2] = Double.PositiveInfinity;
           //cm[2, 3] = 3;
           //cm[2, 4] = 5;
           //cm[3, 0] = 9;
           //cm[3, 1] = 1;
           //cm[3, 2] = 2;
           //cm[3, 3] = Double.PositiveInfinity;
           //cm[3, 4] = 5;
           //cm[4, 0] = 4;
           //cm[4, 1] = 3;
           //cm[4, 2] = 2;
           //cm[4, 3] = 1;
           //cm[4, 4] = Double.PositiveInfinity;

           //List<int> c = new List<int>();

           // BranchAndBoundState test = new BranchAndBoundState(cm, c);

           // System.Diagnostics.Debug.WriteLine("Initial State");
           // test.printMatrix();

           // test.Reduce();

           // System.Diagnostics.Debug.WriteLine("After reduce");
           // test.printMatrix();

           // ArrayList successors = test.GetSuccessors();

           // System.Diagnostics.Debug.WriteLine("Successors:");
           // for (int i = 0; i < successors.Count; i++)
           // {
           //     BranchAndBoundState s = successors[i] as BranchAndBoundState;

           //     s.printMatrix();
           //     System.Diagnostics.Debug.WriteLine("");
           //     ArrayList nextSuccessors = s.GetSuccessors();
           //     System.Diagnostics.Debug.WriteLine("");
           //     System.Diagnostics.Debug.WriteLine("-------Successors of successor----------");
           //     for (int j = 0; j < nextSuccessors.Count; j++)
           //     {
           //         BranchAndBoundState sprime = nextSuccessors[j] as BranchAndBoundState;
           //         System.Diagnostics.Debug.WriteLine("");
           //         sprime.printMatrix();
           //         System.Diagnostics.Debug.WriteLine("");

           //     }

           //     System.Diagnostics.Debug.WriteLine("-----------------------------");

           // }



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(MainForm = new Form1());
        }
        public static Form1 MainForm;

    }
}