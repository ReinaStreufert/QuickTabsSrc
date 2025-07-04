﻿using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Margins = System.Drawing.Printing.Margins;

namespace QuickTabs.Forms
{
    public partial class PrintTab : Form
    {
        public Song Song { get; set; }
        public bool FocusedTrackOnly { get; set; }

        private TabPrinter tabPrinter;
        private PrinterSettings printerSettings;
        private PageSettings pageSettings;
        private QTPrintPreview printPreview = null;
        private bool preferredUseColor = true;
        private bool preferredDoubleSided = false;

        public PrintTab()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            tabPrinter = new TabPrinter();
            tabPrinter.Song = Song;
            tabPrinter.Scale = 0.6F;
            tabPrinter.OnPageCountSet += TabPrinter_OnPageCountSet;
            tabPrinter.FocusedTrackOnly = FocusedTrackOnly;
            this.Cursor = Cursors.WaitCursor;
            printPreview = new QTPrintPreview(tabPrinter.GeneratePreview());
            this.Cursor = Cursors.Default;
            printPreview.Location = previewLocation.Location;
            printPreview.Size = new Size(this.ClientSize.Width - printPreview.Location.X, this.ClientSize.Height - printPreview.Location.Y);
            Controls.Add(printPreview);
            Controls.SetChildIndex(printPreview, 0);
            pageSettings = tabPrinter.Document.DefaultPageSettings;
            printerSettings = tabPrinter.Document.PrinterSettings;
            printerSettings.Duplex = Duplex.Simplex;
            previewZoomInput.Value = (int)(printPreview.Zoom * 100);
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                printerSelect.Items.Add(printerName);
            }
            if (printerSelect.Items.Count > 0)
            {
                printerSelect.SelectedIndex = 0;
            }
            Margins margins = pageSettings.Margins;
            leftMarginInput.Value = margins.Left / (decimal)100;
            rightMarginInput.Value = margins.Right / (decimal)100;
            topMarginInput.Value = margins.Top / (decimal)100;
            bottomMarginInput.Value = margins.Bottom / (decimal)100;
        }

        private void TabPrinter_OnPageCountSet(int count)
        {
            previewPageInput.Maximum = count;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (printPreview == null)
            {
                return;
            }
            printPreview.Location = previewLocation.Location;
            printPreview.Size = new Size(this.ClientSize.Width - printPreview.Location.X, this.ClientSize.Height - printPreview.Location.Y);
        }

        private void refreshPreview()
        {
            this.Cursor = Cursors.WaitCursor;
            printPreview.PreviewSource = tabPrinter.GeneratePreview();
            this.Cursor = Cursors.Default;
        }

        private void invalidatePreview()
        {
            printPreview.InvalidatePreview();
        }

        private void scaleInput_ValueChanged(object sender, EventArgs e)
        {
            tabPrinter.Scale = (float)scaleInput.Value / 100F;
            refreshPreview();
            invalidatePreview();
        }

        private void previewZoomInput_ValueChanged(object sender, EventArgs e)
        {
            printPreview.Zoom = (float)previewZoomInput.Value / 100F;
        }

        private void previewPageInput_ValueChanged(object sender, EventArgs e)
        {
            printPreview.Page = ((int)previewPageInput.Value) - 1;
        }

        private void printerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            printerSettings.PrinterName = PrinterSettings.InstalledPrinters[printerSelect.SelectedIndex];
            if (printerSettings.SupportsColor)
            {
                if (useColorCheck.Checked != preferredUseColor) // not reduntant and heres why: setting the check will call CheckChanged and invalidate the preview, so we should only do it if it needs to be done
                {
                    useColorCheck.Checked = preferredUseColor;
                }
                useColorCheck.ForeColor = DrawingConstants.ContrastColor;
            } else
            {
                if (useColorCheck.Checked)
                {
                    useColorCheck.Checked = false;
                }
                useColorCheck.ForeColor = DrawingConstants.FadedGray;
            }
            if (printerSettings.CanDuplex)
            {
                if (doubleSidedCheck.Checked != preferredDoubleSided)
                {
                    doubleSidedCheck.Checked = preferredDoubleSided;
                }
                doubleSidedCheck.ForeColor = DrawingConstants.ContrastColor;
            } else
            {
                if (doubleSidedCheck.Checked)
                {
                    doubleSidedCheck.Checked = false;
                }
                doubleSidedCheck.ForeColor = DrawingConstants.FadedGray;
            }
            paperSizeSelect.Items.Clear();
            PaperSize currentSize = pageSettings.PaperSize;
            foreach (PaperSize paperSize in printerSettings.PaperSizes)
            {
                int itemIndex = paperSizeSelect.Items.Add(paperSize.PaperName);
                if (paperSize.PaperName == currentSize.PaperName)
                {
                    paperSizeSelect.SelectedIndex = itemIndex;
                }
            }
            if (paperSizeSelect.SelectedIndex == -1)
            {
                paperSizeSelect.SelectedIndex = 0;
            }
            if (copiesInput.Value > printerSettings.MaximumCopies)
            {
                copiesInput.Value = printerSettings.MaximumCopies;
            }
            copiesInput.Maximum = printerSettings.MaximumCopies;
            this.Cursor = Cursors.Default;
        }

        private void useColorCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!printerSettings.SupportsColor)
            {
                if (useColorCheck.Checked)
                {
                    useColorCheck.Checked = false;
                }
                return;
            }
            preferredUseColor = useColorCheck.Checked;
            pageSettings.Color = useColorCheck.Checked;
        }

        private void doubleSidedCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!printerSettings.CanDuplex)
            {
                if (doubleSidedCheck.Checked)
                {
                    doubleSidedCheck.Checked = false;
                }
                return;
            }
            preferredDoubleSided = doubleSidedCheck.Checked;
            if (doubleSidedCheck.Checked)
            {
                tabPrinter.Document.PrinterSettings.Duplex = Duplex.Vertical;
            } else
            {
                printerSettings.Duplex = Duplex.Simplex;
            }
        }

        private void landscapeCheck_CheckedChanged(object sender, EventArgs e)
        {
            pageSettings.Landscape = landscapeCheck.Checked;
            refreshPreview();
            invalidatePreview();
        }

        private void includeCoverCheck_CheckedChanged(object sender, EventArgs e)
        {
            tabPrinter.IncludeCover = includeCoverCheck.Checked;
            refreshPreview();
            invalidatePreview();
        }

        private void marginInputChanged(object sender, EventArgs e)
        {
            Margins margins = new Margins();
            margins.Left = (int)(leftMarginInput.Value * 100);
            margins.Right = (int)(rightMarginInput.Value * 100);
            margins.Top = (int)(topMarginInput.Value * 100);
            margins.Bottom = (int)(bottomMarginInput.Value * 100);
            pageSettings.Margins = margins;
            refreshPreview();
            invalidatePreview();
        }

        private void copiesInput_ValueChanged(object sender, EventArgs e)
        {
            printerSettings.Copies = (short)copiesInput.Value;
        }

        private void paperSizeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            PaperSize newSize = printerSettings.PaperSizes[paperSizeSelect.SelectedIndex];
            if (newSize.PaperName != pageSettings.PaperSize.PaperName)
            {
                pageSettings.PaperSize = newSize;
                refreshPreview();
                invalidatePreview();
            }
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            tabPrinter.Document.Print();
            this.Close();
        }

        private void systemDialogLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogResult dialogResult;
            using (PrintDialog printDialog = new PrintDialog())
            {
                printDialog.Document = tabPrinter.Document;
                dialogResult = printDialog.ShowDialog();
            }
            if (dialogResult == DialogResult.OK)
            {
                pageSettings = tabPrinter.Document.DefaultPageSettings;
                printerSettings = tabPrinter.Document.PrinterSettings;
                preferredDoubleSided = printerSettings.Duplex != Duplex.Simplex;
                preferredUseColor = pageSettings.Color;
                landscapeCheck.Checked = pageSettings.Landscape;
                PrinterSettings.StringCollection installedPrinters = PrinterSettings.InstalledPrinters;
                for (int i = 0; i < installedPrinters.Count; i++)
                {
                    string printerName = installedPrinters[i];
                    if (printerSettings.PrinterName == printerName)
                    {
                        if (printerSelect.SelectedIndex == i)
                        {
                            printerSelect_SelectedIndexChanged(null, null);
                        }
                        else
                        {
                            printerSelect.SelectedIndex = i;
                        }
                        break;
                    }
                }
                Margins margins = pageSettings.Margins;
                leftMarginInput.Value = margins.Left / (decimal)100;
                rightMarginInput.Value = margins.Right / (decimal)100;
                topMarginInput.Value = margins.Top / (decimal)100;
                bottomMarginInput.Value = margins.Bottom / (decimal)100;
                copiesInput.Value = printerSettings.Copies;
                refreshPreview();
                invalidatePreview(); // because who knows what weird thing you changed in the print dialog that this dialog cant detect
            }
        }
    }
}
