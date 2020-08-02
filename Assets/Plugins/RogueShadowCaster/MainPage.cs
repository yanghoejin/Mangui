// converted to unity : http://unitycoder.com/blog/2013/04/06/roguelike-shadowcasting/
// ** remember to donate :) **

// original source:
// http://blogs.msdn.com/b/ericlippert/archive/2011/12/12/shadowcasting-in-c-part-one.aspx

using UnityEngine;
using System;
using System.Linq;
//using System.Windows.Controls;
//using System.Windows.Input;
using System.Text;

namespace SilverlightShadowCasting
{
//    public partial class MainPage
    public class MainPage : MonoBehaviour
    {
		
        private const string MapString = @"
###########################################################
###########################################################
###########################################################
###<....#...........##....##........#####...####........###
###.....#............##...#..........##.....##.....##...###
###.....#.............##......#.......##....##....##....###
#######................##.....##.......##...###...#.....###
####....................############...##..##$..###.....###
###...........#...#...#................##...#####.......###
###.......#...............#.............##.....##.......###
###...........#...#...#.........................#.......###
#####.....#...............#................#....#.......###
###.###.......#...#...#.....................#...#.......###
###...###..................................##..##.......###
###...###..................................###.##.......###
###...###..................................###.##.......###
####....####........##.............#########..#........####
#####..............#####...##......................########
###########################################################
###########################################################
###########################################################
";

        private const char wallMap = '#';
        private const char wallRender = '█';
        private const char floorMap = '.';
        private const char floorRender = '◦';
        private const char playerRender = '@';
        private const char treasureMap = '$';
        private const char stairsMap = '<';

        private int width;
        private int height;
        private char[,] map;
        private int playerX;
        private int playerY;
        private bool hasTreasure;
        private bool hasWon;
		
		private int dx = 0;
        private int dy = 0;
		
		public float fireRate = 0.5F;
		private float nextFire = 0.0F;		
		
		private string mapString;

		private Texture2D tex;
		//private static int gridx;
		//private static int gridy = 15;


		void Start()
		{
			InitializeGame();
			Render();
		}
		
		// void OnGUI() 
//		 {
//			GUI.TextArea(new Rect(0, 0, 700, 700), mapString, 200);
//		}
		

        public MainPage()
        {
            //nitializeComponent();
            //InitializeGame();
//            Render();
        }

        private void InitializeGame()
        {
            var lines = MapString.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            width = lines.Select(x => x.Length).Max();
            height = lines.Length;
            map = new char[width, height];
			
						// create texture
			tex = new Texture2D(width, height);
			GetComponent<Renderer>().material.mainTexture = tex;
			GetComponent<Renderer>().material.mainTexture.filterMode = FilterMode.Point;

			

            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                    map[x, y] = lines[height - y - 1][x];

            playerX = 3;
            playerY = 17;
            hasTreasure = false;
            hasWon = false;
        }

        private string RenderToString()
        {
            bool[,] lit = new bool[width, height];
            int radius = 9;
            ShadowCaster.ComputeFieldOfViewWithShadowCasting(
                playerX, playerY, radius,
                (x1, y1) => map[x1, y1] == wallMap,
                (x2, y2) => { lit[x2, y2] = true; });
            var sb = new StringBuilder();
			
			Color colorFloor = new Color(0.627f, 0.322f, 0.176f,1.0f);
				
            for (int y = height - 1; y >= 0; --y)
            {
                for (int x = 0; x < width; ++x)
                {
					
                    if (x == playerX && y == playerY)
					{
						tex.SetPixel(x,y,Color.blue);
                        sb.Append(playerRender);
					}else if (lit[x, y])
                    {
                        if (map[x, y] == wallMap)
						{
							tex.SetPixel(x,y,Color.gray);
                            sb.Append(wallRender);
						}
                        else if (map[x, y] == floorMap)
						{
							tex.SetPixel(x,y,colorFloor);
                            sb.Append(floorRender);
						}
                        else
						{
							tex.SetPixel(x,y,Color.white);
                            sb.Append(map[x, y]);
						}
                    }
                    else
					{
						tex.SetPixel(x,y,Color.black);
                        sb.Append(' ');
					}
                }
                sb.AppendLine();
            }
			
			tex.Apply(false);
			
            return sb.ToString();
        }

		
		
        private void Render()
        {
          //  label1.Content = RenderToString();
			//mapString = RenderToString();
			//Debug.Log(RenderToString());
			RenderToString();
        }
		
		void Update()
		{
			
			dx=0;
			dy=0;
			
						
			if (Input.GetKey("w") && Time.time > nextFire)
			{
				dy=1;
				nextFire = Time.time + fireRate;
			}
			if (Input.GetKey("s") && Time.time > nextFire)
			{
				dy=-1;
				nextFire = Time.time + fireRate;
			}
			if (Input.GetKey("a") && Time.time > nextFire)
			{
				dx=-1;
				nextFire = Time.time + fireRate;
			}
			if (Input.GetKey("d") && Time.time > nextFire)
			{
				dx=1;
				nextFire = Time.time + fireRate;
			}
			
			// didnt move
			if (dx==0 && dy==0) return;
			
			
			
            if (map[playerX + dx, playerY + dy] != wallMap)
            {
                playerX += dx;
                playerY += dy;
                Render();
            }

			
		}

		/*
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            int dx = 0;
            int dy = 0;

            switch (e.Key)
            {
                case Key.Up: dy = 1; break;
                case Key.Down: dy = -1; break;
                case Key.Left: dx = -1; break;
                case Key.Right: dx = 1; break;
                default: return;
            }

            if (map[playerX + dx, playerY + dy] != wallMap)
            {
                playerX += dx;
                playerY += dy;
                Render();
            }

            if (map[playerX, playerY] == treasureMap)
            {
                hasTreasure = true;
                map[playerX, playerY] = floorMap;
                label1.Content += "\nYou found the treasure! Now get back to the stairs.";
            }

            if (hasTreasure && !hasWon && map[playerX, playerY] == stairsMap)
            {
                hasWon = true;
                label1.Content += "\nYou escaped with the treasure!";
            }
        }*/
    }
}
