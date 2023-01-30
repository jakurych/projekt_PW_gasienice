using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using WPFProgram;

class Program
{
    public static void Start(MainWindow caller) //MainWindow caller, parametr okna
    {

        //sekcje krytyczne
        int [] sk_poziom_tab = new int[7];
        int [] sk_pion_tab = new int[9];

        //tereny gąsienic (niewspółdzielone)
        int[] teren_Z = new int[19];
        int[] teren_N = new int[15];
        int[] teren_B = new int[17];

        //długości gąsienic
        const int size_Z = 5;
        const int size_B = 9;
        const int size_N = 7;

        //pozycja startowa (musi być >= od długości)
        int start_Z = 5;
        int end_Z = start_Z-size_Z;


        int start_B = 9;
        int end_B = start_B-size_B;


        int start_N = 7;
        int end_N = start_N-size_N;

        //semafory
        Semaphore speed = new Semaphore(0, 4);
        Semaphore ogon_N = new Semaphore(0, 2);
        Semaphore ogon_Z = new Semaphore(0, 2);
        Semaphore ogon_B = new Semaphore(0, 2);
        Semaphore sk_poziom = new Semaphore(1, 1);
        Semaphore sk_pion = new Semaphore(1, 1);

        //funkcje
        void ZoltyGlowa()
        {
            //ruch po sqoim terenie
            for (int i = start_Z; i < 19; i++)
            {
                speed.WaitOne(); //-1
                teren_Z[i] = 1;
                ogon_Z.Release(); //+1
                start_Z++;

            }
            //sekcja krytyczna
            sk_poziom.WaitOne();

            for (int i = 0; i < 7; i++)
            {
                speed.WaitOne();
                sk_poziom_tab[i] = 1;
                ogon_Z.Release();

            }
            start_Z = 0;


        }
        void ZoltyOgon()
        {
            for (int i = end_Z; i < 19; i++)
            {
                ogon_Z.WaitOne(); //-1 i puszczenie dalej
                teren_Z[i] = 0;
                end_Z++;
            }
            for (int i = 0; i < 7; i++)
            {
                ogon_Z.WaitOne();
                sk_poziom_tab[i] = 0;
            }
            sk_poziom.Release();

            end_Z = 0;

        }

        void BrazowyGlowa()
        {
            for (int i = start_B; i < 17; i++)
            {
                speed.WaitOne();
                teren_B[i] = 2;
                ogon_B.Release();
            }

            sk_poziom.WaitOne();
            sk_pion.WaitOne();

            for (int i = 6; i >= 3; i--)
            {
                speed.WaitOne();
                sk_poziom_tab[i] = 2;
                ogon_B.Release();
            }

            for (int i = 0; i < 9; i++)
            {
                speed.WaitOne();
                sk_pion_tab[i] = 2;
                ogon_B.Release();

            }

            start_B = 0;
        }

        void BrazowyOgon()
        {
            for (int i = end_B; i < 17; i++)
            {
                ogon_B.WaitOne();
                teren_B[i] = 0;
                end_B++;
            }

            for (int i = 6; i >= 3; i--)
            {
                ogon_B.WaitOne();
                sk_poziom_tab[i] = 0;
            }

            sk_poziom.Release();

            for (int i = 0; i < 9; i++)
            {
                ogon_B.WaitOne();
                sk_pion_tab[i] = 0;

            }
            sk_pion.Release();

            end_B = 0;
        }


        void NiebieskiGlowa()
        {
            for (int i = start_N; i < 15; i++)
            {
                speed.WaitOne();
                teren_N[i] = 3;
                ogon_N.Release();
                start_N++;
            }

            sk_poziom.WaitOne();
            sk_pion.WaitOne();

            for (int i = 8; i >= 0; i--)
            {
                speed.WaitOne();
                sk_pion_tab[i] = 3;
                ogon_N.Release();

            }

            for (int i = 3; i >= 0; i--)
            {
                speed.WaitOne();
                sk_poziom_tab[i] = 3;
                ogon_N.Release();
            }
            start_N = 0;
        }

        void NiebieskiOgon()
        {
            for (int i = end_N; i < 15; i++)
            {
                ogon_N.WaitOne();
                teren_N[i] = 0;
                end_N++;
            }

            for (int i = 8; i >= 0; i--)
            {
                ogon_N.WaitOne();
                sk_pion_tab[i] = 0;

            }

            for (int i = 3; i >= 0; i--)
            {
                ogon_N.WaitOne();
                sk_poziom_tab[i] = 0;
            }
            sk_poziom.Release();
            sk_pion.Release();
            end_N = 0;
        }

        //wątki

        Thread speeder = new Thread(() =>
        {
            while (true)
            {
                speed.Release(4);
                Thread.Sleep(250);
            }
        });

        Thread zoltyGlowaThread = new Thread(() =>
        {
            while (true)
            {
                ZoltyGlowa();
            }
        });

        Thread zoltyOgonThread = new Thread(() =>
        {
            while (true)
            {
                ZoltyOgon();
            }
        });


        Thread brazowyGlowaThread = new Thread(() =>
        {
            while (true)
            {
                BrazowyGlowa();
            }
        });


        Thread brazowyOgonThread = new Thread(() =>
        {
            while (true)
            {
                BrazowyOgon();
            }
        });


        Thread niebieskiGlowaThread = new Thread(() =>
        {
            while (true)
            {
                NiebieskiGlowa();
            }
        });

        Thread niebieskiOgonThread = new Thread(() =>
        {
            while (true)
            {
                NiebieskiOgon();
            }
        });

        Thread window = new Thread(() =>
        {
            while (true)
            {
                speed.WaitOne();
                try
                {
                    //dispatcher pozwala na modyfikacje gui z innego watku niz window
                    caller.Dispatcher.Invoke(delegate
                    {
                        caller.RedrawArea(sk_poziom_tab, sk_pion_tab, teren_Z, teren_B, teren_N);
                    });
                } catch (TaskCanceledException)
                {
                }

            }
        });

        //start watkow
        window.Start();
        zoltyGlowaThread.Start();
        zoltyOgonThread.Start();
        brazowyGlowaThread.Start();
        brazowyOgonThread.Start();
        niebieskiGlowaThread.Start();
        niebieskiOgonThread.Start();
        speeder.Start();
        
    }

}

