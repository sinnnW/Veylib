using System;
using System.Windows.Forms;

namespace Veylib.WinForms.Animations
{
    public class Fade
    {
        public Fade(Form form = null)
        {
            form.Opacity = 0;
            frm = form;
        }

        public enum Mode
        {
            FadeIn,
            FadeOut
        }

        public delegate void noReturn();
        public delegate void modeReturn(Mode mode);
        public delegate void opacityUpdate(double opacity);

        public event modeReturn FadeComplete;
        public event opacityUpdate OpacityUpdate;

        public Form frm;
        private double opacity = 0;

        private void setopacity(double op)
        {
            if (frm.InvokeRequired)
            {
                frm.Invoke((MethodInvoker)(() => { frm.Opacity = op; }));
            }
            else
                frm.Opacity = op;
        }

        // Fade.
        private void fade(Mode mode, bool close = true, bool forceClose = false)
        {
            // Fucky ass opacity fix,
            opacity = mode == Mode.FadeOut ? 1 : 0;

            // Make sure to set form starting out
            OpacityUpdate?.Invoke(opacity);

            // Setup the timer. Fuck timers. They gay
            var tmr = new System.Windows.Forms.Timer();
            tmr.Interval = 15;
            tmr.Tick += (e1, e2) =>
            {
                if (frm == null)
                    tmr.Stop();

                // Fading shit.
                if (mode == Mode.FadeIn)
                {
                    if (frm.Opacity >= 1)
                    {
                        FadeComplete?.Invoke(mode);
                        tmr.Stop();
                    }
                    else
                        setopacity(frm.Opacity + 0.05);
                }
                else
                {
                    if (frm.Opacity <= 0)
                    {
                        // Force closing (if enabled)
                        if (forceClose)
                            Environment.Exit(0);
                        else if (close) // Invoking the event
                            frm.Invoke((MethodInvoker)(() => { frm.Close(); }));

                        // Hide the form
                        //frm.Invoke((MethodInvoker)(() => { frm.Hide(); }));

                        FadeComplete?.Invoke(mode);
                        tmr.Stop();
                    }
                    else
                        setopacity(frm.Opacity - 0.05);
                }

                // Update opacity
                OpacityUpdate?.Invoke(opacity);
            };

            // Show the form if it's fading in.
            if (mode == Mode.FadeIn)
                frm.Invoke((MethodInvoker)(() => { frm.Show(); }));

            // Start the gay ass timer.
            tmr.Start();
        }

        /// <summary>
        /// Fade the control in.
        /// </summary>
        public void In()
        {
            // Duh.
            fade(Mode.FadeIn);
        }

        /// <summary>
        /// Fade the control out.
        /// </summary>
        /// <param name="close">Should it close on 0% opacity</param>
        /// <param name="forceClose">Should it call Environment.Exit after opacity is 0%</param>
        public void Out(bool close = true, bool forceClose = false)
        {
            // Also duh.
            fade(Mode.FadeOut, close, forceClose);
        }
    }
}
