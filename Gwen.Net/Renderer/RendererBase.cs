﻿using System;
using System.IO;

namespace Gwen.Net.Renderer
{
    /// <summary>
    /// Base renderer.
    /// </summary>
    public abstract class RendererBase : IDisposable
    {
        //public Random rnd;
        private Point m_RenderOffset;
        private Rectangle m_ClipRegion;
        private float m_Scale;
        //protected ICacheToTexture m_RTT;

        public float Scale { get { return m_Scale; } set { float oldScale = m_Scale; m_Scale = value; OnScaleChanged(oldScale); } }

        protected abstract void OnScaleChanged(float oldScale);

        /// <summary>
        /// Initializes a new instance of the <see cref="RendererBase"/> class.
        /// </summary>
        protected RendererBase()
        {
            //rnd = new Random();
            m_RenderOffset = Point.Zero;
            m_Scale = 1.0f;
            if (CTT != null)
                CTT.Initialize();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            if (CTT != null)
                CTT.ShutDown();
            GC.SuppressFinalize(this);
        }

#if DEBUG
        ~RendererBase()
        {
            throw new InvalidOperationException(String.Format("IDisposable object finalized: {0}", GetType()));
            //Debug.Print(String.Format("IDisposable object finalized: {0}", GetType()));
        }
#endif

        /// <summary>
        /// Starts rendering.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Stops rendering.
        /// </summary>
        public abstract void End();

        /// <summary>
        /// Gets or sets the current drawing color.
        /// </summary>
        public virtual Color DrawColor { get; set; }

        /// <summary>
        /// Rendering offset. No need to touch it usually.
        /// </summary>
        public Point RenderOffset { get { return m_RenderOffset; } set { m_RenderOffset = value; } }

        /// <summary>
        /// Clipping rectangle.
        /// </summary>
        public Rectangle ClipRegion { get { return m_ClipRegion; } set { m_ClipRegion = value; } }

        /// <summary>
        /// Indicates whether the clip region is visible.
        /// </summary>
        public bool ClipRegionVisible
        {
            get
            {
                if (m_ClipRegion.Width <= 0 || m_ClipRegion.Height <= 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public abstract void DrawLine(int x, int y, int a, int b);

        /// <summary>
        /// Draws a solid filled rectangle.
        /// </summary>
        /// <param name="rect"></param>
        public abstract void DrawFilledRect(Rectangle rect);

        /// <summary>
        /// Starts clipping to the current clipping rectangle.
        /// </summary>
        public abstract void StartClip();

        /// <summary>
        /// Stops clipping.
        /// </summary>
        public abstract void EndClip();

        /// <summary>
        /// Loads the specified texture.
        /// </summary>
        /// <param name="t"></param>
        public abstract void LoadTexture(Texture t);

        /// <summary>
        /// Initializes texture from raw pixel data.
        /// </summary>
        /// <param name="t">Texture to initialize. Dimensions need to be set.</param>
        /// <param name="pixelData">Pixel data in RGBA format.</param>
        public abstract void LoadTextureRaw(Texture t, byte[] pixelData);

        /// <summary>
        /// Initializes texture from image file data.
        /// </summary>
        /// <param name="t">Texture to initialize.</param>
        /// <param name="data">Image file as stream.</param>
        public abstract void LoadTextureStream(Texture t, Stream data);

        /// <summary>
        /// Frees the specified texture.
        /// </summary>
        /// <param name="t">Texture to free.</param>
        public abstract void FreeTexture(Texture t);

        /// <summary>
        /// Draws textured rectangle.
        /// </summary>
        /// <param name="t">Texture to use.</param>
        /// <param name="targetRect">Rectangle bounds.</param>
        /// <param name="u1">Texture coordinate u1.</param>
        /// <param name="v1">Texture coordinate v1.</param>
        /// <param name="u2">Texture coordinate u2.</param>
        /// <param name="v2">Texture coordinate v2.</param>
        public abstract void DrawTexturedRect(Texture t, Rectangle targetRect, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1);

        /// <summary>
        /// Draws "missing image" default texture.
        /// </summary>
        /// <param name="rect">Target rectangle.</param>
        public virtual void DrawMissingImage(Rectangle rect)
        {
            //DrawColor = Color.FromArgb(255, rnd.Next(0,255), rnd.Next(0,255), rnd.Next(0, 255));
            DrawColor = Color.Red;
            DrawFilledRect(rect);
        }

        /// <summary>
        /// Cache to texture provider.
        /// </summary>
        public virtual ICacheToTexture CTT { get { return null; } }

        /// <summary>
        /// Loads the specified font.
        /// </summary>
        /// <param name="font">Font to load.</param>
        /// <returns>True if succeeded.</returns>
        public abstract bool LoadFont(Font font);

        /// <summary>
        /// Frees the specified font.
        /// </summary>
        /// <param name="font">Font to free.</param>
        public abstract void FreeFont(Font font);

        /// <summary>
        /// Gets the font metrics.
        /// </summary>
        /// <param name="font">Font.</param>
        /// <returns>The font metrics.</returns>
        public abstract FontMetrics GetFontMetrics(Font font);

        /// <summary>
        /// Returns dimensions of the text using specified font.
        /// </summary>
        /// <param name="font">Font to use.</param>
        /// <param name="text">Text to measure.</param>
        /// <returns>Width and height of the rendered text.</returns>
        public abstract Size MeasureText(Font font, string text);

        /// <summary>
        /// Renders text using specified font.
        /// </summary>
        /// <param name="font">Font to use.</param>
        /// <param name="position">Top-left corner of the text.</param>
        /// <param name="text">Text to render.</param>
        public abstract void RenderText(Font font, Point position, string text);

        //
        // No need to implement these functions in your derived class, but if 
        // you can do them faster than the default implementation it's a good idea to.
        //

        /// <summary>
        /// Draws a lined rectangle. Used for keyboard focus overlay.
        /// </summary>
        /// <param name="rect">Target rectangle.</param>
        public abstract void DrawLinedRect(Rectangle rect);

        /// <summary>
        /// Draws a single pixel. Very slow, do not use. :P
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        public virtual void DrawPixel(int x, int y)
        {
            //[omeg] amazing;)
            DrawFilledRect(new Rectangle(x, y, 1, 1));
        }

        /// <summary>
        /// Gets pixel color of a specified texture. Slow.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <returns>Pixel color.</returns>
        public virtual Color PixelColor(Texture texture, uint x, uint y)
        {
            return PixelColor(texture, x, y, Color.White);
        }

        /// <summary>
        /// Gets pixel color of a specified texture, returning default if otherwise failed. Slow.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="defaultColor">Color to return on failure.</param>
        /// <returns>Pixel color.</returns>
        public abstract Color PixelColor(Texture texture, uint x, uint y, Color defaultColor);

        /// <summary>
        /// Draws a round-corner rectangle.
        /// </summary>
        /// <param name="rect">Target rectangle.</param>
        /// <param name="slight"></param>
        public abstract void DrawShavedCornerRect(Rectangle rect, bool slight = false);


        private int TranslateX(int x)
        {
            int x1 = x + m_RenderOffset.X;
            return x1;
        }

        private int TranslateY(int y)
        {
            int y1 = y + m_RenderOffset.Y;
            return y1;
        }

        /// <summary>
        /// Translates a panel's local drawing coordinate into view space, taking offsets into account.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Translate(ref int x, ref int y)
        {
            x += m_RenderOffset.X;
            y += m_RenderOffset.Y;
        }

        /// <summary>
        /// Translates a panel's local drawing coordinate into view space, taking offsets into account.
        /// </summary>
        public Point Translate(Point p)
        {
            int x = p.X;
            int y = p.Y;
            Translate(ref x, ref y);
            return new Point(x, y);
        }

        /// <summary>
        /// Translates a panel's local drawing coordinate into view space, taking offsets into account.
        /// </summary>
        public Rectangle Translate(Rectangle rect)
        {
            return new Rectangle(TranslateX(rect.X), TranslateY(rect.Y), rect.Width, rect.Height);
        }

        /// <summary>
        /// Adds a point to the render offset.
        /// </summary>
        /// <param name="offset">Point to add.</param>
        public void AddRenderOffset(Rectangle offset)
        {
            m_RenderOffset = new Point(m_RenderOffset.X + offset.X, m_RenderOffset.Y + offset.Y);
        }

        /// <summary>
        /// Adds a rectangle to the clipping region.
        /// </summary>
        /// <param name="rect">Rectangle to add.</param>
        public void AddClipRegion(Rectangle rect)
        {
            rect.X = m_RenderOffset.X;
            rect.Y = m_RenderOffset.Y;

            Rectangle r = rect;
            if (rect.X < m_ClipRegion.X)
            {
                r.Width -= (m_ClipRegion.X - r.X);
                r.X = m_ClipRegion.X;
            }

            if (rect.Y < m_ClipRegion.Y)
            {
                r.Height -= (m_ClipRegion.Y - r.Y);
                r.Y = m_ClipRegion.Y;
            }

            if (rect.Right > m_ClipRegion.Right)
            {
                r.Width = m_ClipRegion.Right - r.X + 1;
            }

            if (rect.Bottom > m_ClipRegion.Bottom)
            {
                r.Height = m_ClipRegion.Bottom - r.Y + 1;
            }

            m_ClipRegion = r;
        }

        /// <summary>
        /// Sets a rectangle to the clipping region.
        /// </summary>
        /// <param name="rect">Rectangle to set.</param>
        public void SetClipRegion(Rectangle rect)
        {
            rect.X += m_RenderOffset.X;
            rect.Y += m_RenderOffset.Y;

            Rectangle r = rect;
            if (rect.X < m_ClipRegion.X)
            {
                r.Width -= (m_ClipRegion.X - r.X);
                r.X = m_ClipRegion.X;
            }

            if (rect.Y < m_ClipRegion.Y)
            {
                r.Height -= (m_ClipRegion.Y - r.Y);
                r.Y = m_ClipRegion.Y;
            }

            if (rect.Right > m_ClipRegion.Right)
            {
                r.Width = m_ClipRegion.Right - r.X + 1;
            }

            if (rect.Bottom > m_ClipRegion.Bottom)
            {
                r.Height = m_ClipRegion.Bottom - r.Y + 1;
            }

            m_ClipRegion = r;
        }
    }
}