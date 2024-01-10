using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace milano88.UI.Controls
{
    [DefaultEvent("ProgressChanged")]

    public class MSCircleProgressBar : Control
    {
        public MSCircleProgressBar()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(100, 100);
            this.Font = new Font("Segoe UI", 9f);
            this.BackColor = Color.Transparent;
            this.DoubleBuffered = true;
        }

        #region EventHandler
        [Description("Occurs when the progress property has changed and the control has invalidated")]
        public event EventHandler ProgressChanged;
        [Description("Occurs when progress reaches 100%")]
        public event EventHandler ProgressCompleted;
        #endregion

        #region Paint
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float pixelPercent = 360F * (_value / _maximum);
            Rectangle rectGradient = new Rectangle(Point.Empty, this.Size);

            using (SolidBrush brushParrent = new SolidBrush(this.Parent.BackColor))
            using (SolidBrush brushProgressBack = new SolidBrush(_progressColor))
            using (LinearGradientBrush brushPercent = new LinearGradientBrush(rectGradient, _progressColor1, _progressColor2, 360f))
            using (Pen penProgress = new Pen(brushProgressBack, _barSize - 2F))
            using (Pen penPercent = new Pen(brushPercent, _barSize - 1F))
            {
                graphics.DrawArc(penProgress, _barSize / 2F, _barSize / 2F, this.Width - _barSize, this.Height - _barSize, -90F, 360F);

                if (pixelPercent > 0F)
                {
                    if (_roundedBar)
                    {
                        penPercent.StartCap = LineCap.Round;
                        penPercent.EndCap = LineCap.Round;
                    }
                    graphics.DrawArc(penPercent, _barSize / 2F, _barSize / 2F, Width - _barSize, Height - _barSize, -90F, pixelPercent);
                }

                if (_showText)
                {
                    var format = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.GlyphOverhangPadding;
                    TextRenderer.DrawText(graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, format);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (Parent != null && BackColor == Color.Transparent)
            {
                Rectangle rect = new Rectangle(Left, Top, Width, Height);
                pevent.Graphics.TranslateTransform(-rect.X, -rect.Y);
                try
                {
                    using (PaintEventArgs pea = new PaintEventArgs(pevent.Graphics, rect))
                    {
                        pea.Graphics.SetClip(rect);
                        InvokePaintBackground(Parent, pea);
                        InvokePaint(Parent, pea);
                    }
                }
                finally
                {
                    pevent.Graphics.TranslateTransform(rect.X, rect.Y);
                }
            }
            else
            {
                using (SolidBrush backColor = new SolidBrush(this.BackColor))
                    pevent.Graphics.FillRectangle(backColor, ClientRectangle);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.Height < 100) this.Height = 100;
            if (this.Width < 100) this.Width = 100;
            this.Height = this.Width;
        }
        #endregion

        #region Properties
        [Category("Custom Properties")]
        [DefaultValue(typeof(Color), "Black")]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                base.ForeColor = value;
                this.Invalidate();
            }
        }

        [Category("Custom Properties")]
        [DefaultValue(typeof(Font), "Segoe UI, 9pt")]
        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                this.Invalidate();
            }
        }

        private Color _progressColor = Color.White;
        private Color _progressColor1 = Color.Fuchsia;
        private Color _progressColor2 = Color.YellowGreen;
        private float _barSize = 20F;

        [Category("Custom Properties")]
        [DefaultValue(20)]
        public float BarSize
        {
            get => _barSize;
            set
            {
                _barSize = value;
                this.Invalidate();
            }
        }

        [Category("Custom Properties")]
        [DefaultValue(typeof(Color), "White")]
        public Color ProgressColor
        {
            get { return _progressColor; }
            set
            {
                _progressColor = value;
                this.Invalidate();
            }
        }

        [Category("Custom Properties")]
        [DefaultValue(typeof(Color), "Fuchsia")]
        public Color ProgressColor1
        {
            get { return _progressColor1; }
            set
            {
                _progressColor1 = value;
                this.Invalidate();
            }
        }

        [Category("Custom Properties")]
        [DefaultValue(typeof(Color), "YellowGreen")]
        public Color ProgressColor2
        {
            get { return _progressColor2; }
            set
            {
                _progressColor2 = value;
                this.Invalidate();
            }
        }

        [Category("Custom Properties")]
        [DefaultValue("")]
        public override string Text { get => base.Text; set { base.Text = value; this.Invalidate(); } }

        [Category("Custom Properties")]
        [DefaultValue(typeof(Color), "Transparent")]
        public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

        private int _maximum = 100;
        [Category("Custom Properties")]
        [DefaultValue(100)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (value <= _minimum)
                    throw new ArgumentOutOfRangeException("Value must be greater than Minimum");
                _maximum = value;
                if (_value > _maximum)
                    Value = _maximum;
                this.Invalidate();
            }
        }

        private int _minimum = 0;
        [Category("Custom Properties")]
        [DefaultValue(0)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (value >= _maximum)
                    throw new ArgumentOutOfRangeException("Value must be less than Maximum");
                _minimum = value;
                if (_value < _minimum)
                    Value = _minimum;
                this.Invalidate();
            }
        }

        private float _value = 0;
        [Category("Custom Properties")]
        [DefaultValue(0)]
        public float Value
        {
            get { return _value; }
            set
            {
                if (value < _minimum || value > _maximum)
                    throw new ArgumentOutOfRangeException("value must be less than or equal to Maximum and greater than or equal to Minimum");
                if (value >= _maximum)
                {
                    _value = _maximum;
                    ProgressCompleted?.Invoke(this, EventArgs.Empty);
                }
                else if (value < 0) _value = 0;

                bool changed = value != _value;
                if (changed)
                {
                    _value = value;
                    ProgressChanged?.Invoke(this, EventArgs.Empty);
                }
                this.Invalidate();
            }
        }

        private bool _roundedBar = false;
        [Category("Custom Properties")]
        [DefaultValue(typeof(bool), "False")]
        public bool RoundedBar
        {
            get => _roundedBar;
            set
            {
                _roundedBar = value;
                this.Invalidate();
            }
        }

        private bool _showText = false;
        [Category("Custom Properties")]
        [DefaultValue(typeof(bool), "False")]
        public bool ShowText
        {
            get => _showText;
            set
            {
                _showText = value;
                this.Invalidate();
            }
        }

        #endregion
    }
}

