using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Veylib.CLIUI
{
    public class AsciiTable
    {
        public class ColorProperties
        {
            public ColorProperties()
            {
                HeaderColor = Color.White;
                ContentColor = Color.WhiteSmoke;
                PipeColor = Color.White;
            }

            public Color HeaderColor;
            public Color ContentColor;
            public Color PipeColor;

            public bool RainbowDividers = false;
        }

        public class Properties
        {
            public Properties()
            {
                if (Colors == null)
                    Colors = new ColorProperties();
            }

            public bool ShowDividers = true;
            public ColorProperties Colors;
            public bool CenterOnWrite = false;
        }

        public AsciiTable(int rows, int columns)
        {
            Property = new Properties();

            // alotting the amount
            Rows = new List<Row>(rows);
            Columns = new List<Column>(columns);
        }

        public AsciiTable(Properties prop = null)
        {
            Property = new Properties();

            if (prop == null)
                prop = new Properties();

            // init new lists for these
            Rows = new List<Row>();
            Columns = new List<Column>();

            // import properties
            Property = prop;
        }

        public AsciiTable(params string[] columns)
        {
            Property = new Properties();

            Rows = new List<Row>();
            Columns = new List<Column>();

            foreach (var col in columns)
                Columns.Add(new Column(this, col));
        }

        public List<Row> Rows;
        public List<Column> Columns;
        public int RowIdentifier = 0;
        public int ColumnIdentifier = 0;

        public Properties Property;

        public static readonly Dictionary<string, char> PipesDictionary = new Dictionary<string, char>
        {
            { "downright", '╔' },
            { "downleft", '╗' },
            { "leftright", '═' },
            { "updown", '║' },
            { "upright", '╚' },
            { "upleft", '╝' },
            { "downleftright", '╦' },
            { "upleftright", '╩' },
            { "updownleft", '╣' },
            { "updownright", '╠' },
            { "updownleftright", '╬' }
        };

        public class Row
        {
            public Row(AsciiTable parent, params string[] columns)
            {
                if (columns.Length > parent.Columns.Count)
                    throw new ArgumentOutOfRangeException("Too many columns were supplied for the row!");

                Cells = new List<object>();
                Parent = parent;
                Identifier = parent.RowIdentifier;
                parent.RowIdentifier++;

                foreach (var col in columns)
                    Cells.Add(col ?? "");
            }

            public List<dynamic> Cells;
            public AsciiTable Parent;
            public int Identifier;

            public void Remove()
            {
                Parent.RemoveRowAt(Identifier);
            }
        }

        public class Column
        {
            public Column(AsciiTable parent, string header)
            {
                Header = header;
                Identifier = parent.ColumnIdentifier;
                Parent = parent;
                parent.ColumnIdentifier++;
            }

            public string Header;
            public int Identifier;
            public AsciiTable Parent;

            public void Remove()
            {
                Parent.RemoveColumnAt(Identifier);
            }
        }


        public void AddColumn(Column col)
        {
            Columns.Add(col);
        }

        public void AddColumn(string header)
        {
            Columns.Add(new Column(this, header));
        }

        public void RemoveColumnAt(int identifier)
        {
            Columns.RemoveAt(identifier);
        }

        public void AddRow(Row row)
        {
            Rows.Add(row);
        }

        public void AddRow(params string[] columns)
        {
            Rows.Add(new Row(this, columns));
        }

        public void RemoveRowAt(int identifier)
        {
            Rows.RemoveAt(identifier);
        }

        private static int tableWidth = 0;
        private static int colWidth = 0;

        private static class build
        {
            public static string topLine(List<Column> columns)
            {
                PipesDictionary.TryGetValue("downright", out char bottomright);
                PipesDictionary.TryGetValue("downleft", out char bottomleft);
                PipesDictionary.TryGetValue("leftright", out char leftright);
                PipesDictionary.TryGetValue("downleftright", out char downleftright);

                string ret = bottomright.ToString();

                for (var x = 0; x < columns.Count; x++)
                {
                    if (x > 0)
                        ret += downleftright;

                    ret += new string(leftright, colWidth - 1);
                }
                ret += bottomleft;

                return ret;
            }

            public static string bottomLine(List<Column> columns)
            {
                PipesDictionary.TryGetValue("upright", out char upright);
                PipesDictionary.TryGetValue("upleft", out char upleft);
                PipesDictionary.TryGetValue("leftright", out char leftright);
                PipesDictionary.TryGetValue("upleftright", out char upleftright);

                string ret = upright.ToString();

                for (var x = 0; x < columns.Count; x++)
                {
                    if (x > 0)
                        ret += upleftright;

                    ret += new string(leftright, colWidth - 1);
                }
                ret += upleft;

                return ret;
            }

            public static string divider(List<Column> columns)
            {
                PipesDictionary.TryGetValue("updownright", out char updownright);
                PipesDictionary.TryGetValue("updownleft", out char updownleft);
                PipesDictionary.TryGetValue("leftright", out char leftright);
                PipesDictionary.TryGetValue("updownleftright", out char updownleftright);

                string ret = updownright.ToString();

                for (var x = 0; x < columns.Count; x++)
                {
                    if (x > 0)
                        ret += updownleftright;

                    ret += new string(leftright, colWidth - 1);
                }
                ret += updownleft;

                return ret;
            }
        }

        public override string ToString()
        {
            // set the table width
            foreach (var col in Columns)
                if (col.Header.Length > colWidth)
                    colWidth = col.Header.Length + 4;

            foreach (var row in Rows)
                foreach (var cell in row.Cells)
                    if (cell.ToString().Length > colWidth)
                        colWidth = cell.ToString().Length + 3;

            tableWidth = colWidth * Columns.Count;
            Debug.WriteLine($"Using {tableWidth} char width");

            var lines = new List<string>();

            // top
            lines.Add(build.topLine(Columns));

            // get the up down pipe
            PipesDictionary.TryGetValue("updown", out char updown);

            // columns
            List<string> cols = new List<string>();
            foreach (var col in Columns)
            {
                int len = colWidth - col.Header.Length - 3;
                cols.Add(len > 0 ? $"{col.Header}{new string(' ', len)}" : col.Header);
            }

            // add the actual columns
            lines.Add($"{updown} {string.Join($" {updown} ", cols)} {updown}");

            // rows
            foreach (var row in Rows)
            {
                if (Property.ShowDividers)
                    lines.Add(build.divider(Columns));

                var strb = new StringBuilder();
                List<string> cellsFormatted = new List<string>();

                if (row.Cells.Count < Columns.Count)
                    for (var x = 0; x < (Columns.Count - row.Cells.Count) + 1; x++)
                        row.Cells.Add(" ");

                Debug.WriteLine(row.Cells.Count.ToString());

                for (var x = 0; x < row.Cells.Count; x++)
                    cellsFormatted.Add($"{row.Cells[x]}{(colWidth - row.Cells[x].ToString().Length > 0 ? new string(' ', colWidth - row.Cells[x].ToString().Length - 3) : "")}");

                // Remove those temp cells
                row.Cells.RemoveAll(cell => cell.ToString().Length == 0);

                //// remaining cells
                //if ((Columns.Count - row.Cells.Count) > 0)
                //{
                //    for (var x = 0; x < Columns.Count - row.Cells.Count; x++)
                //        strb.Append($"{new string(' ', colWidth - 3)}");
                //    //strb.Remove(strb.Length - 2, 2);

                //    cellsFormatted.Add(strb.ToString());
                //}


                lines.Add($"{updown} {string.Join($" {updown} ", cellsFormatted)} {updown}");

                //strb.Append(updown);
                //lines.Add($"{updown} {string.Join(" | ", row.Cells)} {updown}");
            }


            //lines.Add($"");


            // bottom
            lines.Add(build.bottomLine(Columns));

            Debug.WriteLine(string.Join("\n", lines));

            return string.Join("\n", lines);
        }

        public void WriteTable()
        {
            Core core = Core.GetInstance();

            string[] lines = ToString().Split('\n');
            PipesDictionary.TryGetValue("updown", out char updown);
            PipesDictionary.TryGetValue("updownright", out char updownright);

            for (var x = 0; x < lines.Length; x++)
            {
                if (x == 0 || x == lines.Length - 1 || lines[x].StartsWith(updownright.ToString()))
                    if (Property.Colors.RainbowDividers)
                        core.WriteLine(new Core.MessageProperties { Label = new Core.MessagePropertyLabel { Show = false }, Time = new Core.MessagePropertyTime { Show = false }, VerticalRainbow = true, Center = true }, lines[x]);
                    else
                        core.WriteLine(new Core.MessageProperties { Label = new Core.MessagePropertyLabel { Show = false }, Time = new Core.MessagePropertyTime { Show = false }, Center = true }, Property.Colors.PipeColor, lines[x]);
                else if (lines[x].StartsWith(updown.ToString()))
                {
                    StringBuilder sb = new StringBuilder();
                    Core.MessageProperties mp = new Core.MessageProperties { Label = new Core.MessagePropertyLabel { Show = false }, Time = new Core.MessagePropertyTime { Show = false }, Center = true };
                    for (var y = 0; y < lines[x].Length; y++)
                    {
                        if (lines[x][y] == updown)
                        {
                            if (sb.Length > 0)
                            {
                                mp.ColoringGroups.Add(new object[] { Property.Colors.ContentColor, sb.ToString() });
                                sb.Clear();
                            }

                            if (Property.Colors.RainbowDividers)
                                mp.ColoringGroups.Add(new object[] { "rainbow", updown });
                            else
                                mp.ColoringGroups.Add(new object[] { Property.Colors.PipeColor, updown });
                        }
                        else
                            sb.Append(lines[x][y]);
                    }
                    core.WriteLine(mp);
                }
                else
                    core.WriteLine(new Core.MessageProperties { Label = new Core.MessagePropertyLabel { Show = false }, Time = new Core.MessagePropertyTime { Show = false }, Center = true }, lines[x]);
            }
        }
    }
}