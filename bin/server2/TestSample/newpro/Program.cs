///////////////////////////////////////////////////////////////////////////////
///  Program.cs  - Test BlockingQueue                                       ///
///  ver 3.0                                                                ///
///  Language:     C#, VS 2012                                              ///
///  Platform:     Dell Dimension 8100, Windows 2000 Pro, SP2               ///
///  Application:  Demonstration for CSE681 - Software Modeling & Analysis  ///
///  Author:       Jim Fawcett, CST 4-187, Syracuse University              ///
///                (315) 443-3948, jfawcett@twcny.rr.com                    ///
///////////////////////////////////////////////////////////////////////////////
/*
 *   Note that the ThreadProcXXX classes, below, provide a convenient way to
 *   pass data into a thread before it starts.
 */
using System;
using System.Threading;

namespace SWTools
{
  class Program
  {
    class ThreadProcEnQ  // enQueuing thread processing
    {
      BlockingQueue<string> Q;
      public ThreadProcEnQ(BlockingQueue<string> bQ)
      {
        Q = bQ;
      }
      public void proc()  // semantics for ThreadStart
      {
        object input = 0;
        for (int i = 0; i < 20; ++i)
        {
          input = i;
          Console.Write(
            "\n Child thread #{0} enQing message {1}, then sleeping",
            Thread.CurrentThread.ManagedThreadId,
            input.ToString()
          );
          Q.enQ(input.ToString());
          Thread.Sleep(500);
        }
        Console.Write(
          "\n Child thread #{0} enQing message {1}, then sleeping",
          Thread.CurrentThread.ManagedThreadId, 
          input.ToString()
        );
        Q.enQ("quit");
      }
    }
    //
    class ThreadProcDeQ   // deQueuing thread processing
    {
      bool quit = false;
      BlockingQueue<string> Q;
      public ThreadProcDeQ(BlockingQueue<string> bQ)
      {
        Q = bQ;
      }
      public void proc()   // semantics for ThreadStart
      {
        string msg = "";
        int i = 0;
        while (!quit || Q.size() > 0)
        {
          object input = ++i;
          msg = Q.deQ();
          Console.Write(
            "\n Child thread #{0} deQed message {1}", 
            Thread.CurrentThread.ManagedThreadId, msg
          );
          quit = (msg == "quit");
        }
        Q.enQ("quit");  // wake up other thread
      }
    }
    //
    //----< blocking queue test stub >---------------------------------

    static void Main()
    {
      Console.Write("\n  Test Blocking Queue\n");

      BlockingQueue<string> bQ = new BlockingQueue<string>();
      
      // enQ thread processing

      ThreadProcEnQ tpEQ = new ThreadProcEnQ(bQ);
      Thread te = new Thread(tpEQ.proc);
      te.Start();

      // deQ thread processing

      ThreadProcDeQ tpDQ = new ThreadProcDeQ(bQ);

      Thread td1 = new Thread(tpDQ.proc);
      td1.Start();

      Thread td2 = new Thread(tpDQ.proc);
      td2.Start();

      td1.Join();
      td2.Join();
      
      // the last deQ thread's quit message still in Q

      Console.Write("\n  Queue size = {0}", bQ.size());
      bQ.clear();
      Console.Write("\n  after purging size = {0}", bQ.size());
      
      Console.Write("\n\n");
    }
  }
}
