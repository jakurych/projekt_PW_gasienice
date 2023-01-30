using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFProgram
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Linia Pozioma
        public static Line newHline(double x1, double y1, double w, int offsX, int offsY) //nowa pozioma linia na pozycji x1, y1 o dlugosci w
        {
            Line a = new Line();
            a.Fill = Brushes.Black;
            a.StrokeThickness = 2;
            a.Stroke = Brushes.Black;
            a.X1 = offsX + x1;
            a.X2 = offsX+x1+w;
            a.Y1 = offsY + y1;
            a.Y2 = offsY + y1;
            return a;
        }
        //Linia Pionowa
        public static Line newVline(double x1, double y1, double h, int offsX, int offsY)
        {
            Line a = new Line();
            a.Fill = Brushes.Black;
            a.StrokeThickness = 2;
            a.Stroke = Brushes.Black;
            a.X1 = offsX + x1;
            a.X2 = offsX + x1;
            a.Y1 = offsY + y1;
            a.Y2 = offsY + y1+h;
            return a;
        }

        readonly string[] map = new string[] { //pomocniczy string -> when # rysuj 2x Vline 2x Hline wiec kwadrat
            "  #######   ",
            "  #     #   ",
            "  #     #   ",
            "  #     #   ",
            "  #     #   ",
            "  #     #   ",
            "  #     #   ",
            "############",
            "#    #     #",
            "#    #     #",
            "#    #     #",
            "#    #     #",
            "#    #     #",
            "#    #     #",
            "#    #     #",
            "#    #     #",
            "############",
        };

        public void RedrawAreaOutline()
        {

            int offsetX = 50;
            int offsetY = 20;
            double w = outline.ActualWidth - offsetX * 2;
            double h = outline.ActualHeight - offsetY * 2;

            if (w <= 0 || h <= 0)
            {
                return;
            }

            double boxH = h / 17;
            double boxW = w / 12;

           outline.Children.Clear();

            for (int i = 0; i != map.Length; i++)
            {
                string s = map[i];
                for (int j = 0; j != s.Length; j++)
                {
                    char x = s[j];
                    if (x == '#')
                    {
                        bool lastInX = j == s.Length - 1; //czy ostatni w rzedzie
                        bool lastInY = i == map.Length - 1; //czy ostatni w kolumnie
                        outline.Children.Add(newHline(boxW * j, boxH * i, boxW, offsetX, offsetY)); //zrobienie górnej
                        if (lastInY || map[i + 1][j] != '#') //czy zrobic dolna?
                        {
                            outline.Children.Add(newHline(boxW * j, boxH * (i + 1), boxW, offsetX, offsetY));
                        }
                        outline.Children.Add(newVline(boxW * j, boxH * i, boxH, offsetX, offsetY));
                        if (lastInX || s[j + 1] != '#') //czy zrobić prawą?
                        {
                            outline.Children.Add(newVline(boxW * (j + 1), boxH * i, boxH, offsetX, offsetY));
                        }
                    }
                }
            }
        }

        public void RedrawArea(int[] poziom, int[] pion, int[] terenZ, int[] terenB, int[] terenN)
        {

            //cały obszar
            int[,] space = new int[12,17];

            //sekcja krytyczna poziom
            for (int x = 0; x != 7; x++)
            {
                space[2+ x, 7] = poziom[x];
            }

            //sekcja krytyczna pion
            for (int y = 0; y != 9; y++)
            {
                space[5, 8 + y] = pion[y];
            }

            //teren brazowgo
            for (int teren_B = 0; teren_B != 17; teren_B++) //wypelnianie koloru dla brazowego
            {
                //Jak iść?
                if (teren_B < 6)
                {
                    space[6 + teren_B, 16] = terenB[teren_B];
                } else if (teren_B >= 14)
                {
                    space[11 - (teren_B - 14), 7] = terenB[teren_B];
                } else
                {
                    space[11, 16 - (teren_B - 5)] = terenB[teren_B];
                }
            }

            //dzialka niebieskiego
            for (int teren_N = 0; teren_N != 15; teren_N++)
            {
                if (teren_N < 2)
                {
                    space[1 - teren_N, 7] = terenN[teren_N];
                } else if (teren_N >= 10)
                {
                    space[teren_N - 10, 16] = terenN[teren_N];
                } else
                {
                    space[0, 7 + (teren_N-1)] = terenN[teren_N];
                }
            }

            //dzialka żółtego
            for (int teren_Z = 0; teren_Z != 19; teren_Z++)
            {
                if (teren_Z < 7)
                {
                    space[8, 6 - teren_Z] = terenZ[teren_Z];
                } else if (teren_Z >= 12)
                {
                    space[2, (teren_Z-12)] = terenZ[teren_Z];
                } else
                {
                    space[7 - (teren_Z - 7), 0] = terenZ[teren_Z];
                }
            }

            colorFills.Children.Clear();

            //marginesy, szerokosci,wysokosci
            int offsetX = 50;
            int offsetY = 20;
            double w = outline.ActualWidth - offsetX * 2;
            double h = outline.ActualHeight - offsetY * 2;
            double boxH = h / 17;
            double boxW = w / 12;

            if (w <= 0 || h <= 0)
            {
                return;
            }

            for (int x = 0; x != 12; x++)
            {
                for (int y = 0; y != 17; y++)
                {
                    if (space[x,y] != 0)
                    {
                        int colorcode = space[x, y];
                        Rectangle nrect = new Rectangle();
                        nrect.Width = boxW;
                        nrect.Height = boxH;
                        nrect.Fill =
                            colorcode == 1 ? Brushes.Yellow :
                            colorcode == 2 ? Brushes.Brown :
                            colorcode == 3 ? Brushes.Blue :
                            Brushes.White;
                        colorFills.Children.Add(nrect);
                        Canvas.SetTop(nrect, offsetY + boxH * y);
                        Canvas.SetLeft(nrect, offsetX + boxW * x);

                        /*
                           Metoda "SetTop" ustawia górną krawędź prostokąta "nrect" na wartość "offsetY + boxH * y".
                           Wartość "offsetY" to stały offset w osi Y, a "boxH * y" to wartość współrzędnej Y obliczona jako iloczyn wysokości "boxH" i współczynnika "y".
                           Metoda "SetLeft" ustawia lewą krawędź prostokąta "nrect" na wartość "offsetX + boxW * x".
                           Wartość "offsetX" to stały offset w osi X, a "boxW * x" to wartość współrzędnej X obliczona jako iloczyn szerokości "boxW" i współczynnika "x".
                           W rezultacie, pozycja "nrect" na canvasie jest ustawiona za pomocą sumy stałych offsetów i współczynników dla współrzędnych X i Y.
                         */
                    }
                }
            }

        }

        public MainWindow()
        {
            Program.Start(this);

            InitializeComponent();
            Loaded += delegate
            {
                RedrawAreaOutline();
            };
            SizeChanged += delegate
            {
                RedrawAreaOutline();
                
            };

        }

        //zamknięcie całości
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}
