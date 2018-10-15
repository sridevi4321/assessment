using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accusoft.PdfXpressSdk;
using System.IO;
using System.Drawing;




namespace PDFXpressConvertPDF2Image
{
    class PDFXpressConverter
    {
        private PdfXpress pdfXpress;


        /*
         * Class constructor.  
         * 
         * Initiates the Accusoft.PdfXpressSdk.PdfXpress font and character map path.
         * 
         * Please note that this program assumes the existance of the "library" folder 
         * with the Font and CMap copied from the PDFXpress DotNet SDK directory located 
         * under:
         * 
         * [PDFXpress DotNet install directory]\PDFXpress\V7.0\Support
         * 
         * */
        public PDFXpressConverter()
        {
            pdfXpress = new Accusoft.PdfXpressSdk.PdfXpress();
            string resourcePath = Environment.CurrentDirectory + @"\library\";
            System.Console.WriteLine(resourcePath);
            string fontPath = resourcePath + "Font";
            string cmapPath = resourcePath + "CMap";
            //init the PDFXPress object
            pdfXpress.Initialize(fontPath, cmapPath);
        }


        /*
         * Deconstructor.  
         * Clears the Accusoft.PdfXpressSdk.PdfXpress object
         * if it hasn't been done already.
         * 
         * */
        ~PDFXpressConverter()
        {
            //properly dispose of our PDF object
            if (pdfXpress != null)
            {
                pdfXpress.Dispose();
                pdfXpress = null;
            }


        }


        /*
         * Progress display.
         * 
         * Shows the current element (index) compared to the final element (end).
         * 
         * Parameters:
         *      index:  System.Int32, tracks the current item being converted.
         *      end:    System.Int32, shows the final item to be converted.
         * 
         * Returns:  Nothing.
         * 
         * */
        private void Progress(System.Int32 index, System.Int32 end)
        {
            Console.CursorLeft = 0;
            System.Console.Write("Converting " + index.ToString() + " out of " + end.ToString());
        }






        /*
         * ConvertPDF2Image
         * 
         * Uses the Accusoft.PdfXpressSdk.PdfXpress to load a PDF file named "PDFFileName".  It then parses
         * through the pages starting at pageStart to pageEnd.  Each PDF page parsed is saved
         * as the format specified (bmp, png, gif, with jpg as the default) with the following naming convention:
         * 
         * PDFFileName[CurrentPageNumber].format.
         * 
         * CurrentPageNumber is the page number being converted, with trailing 0s if the number is less 
         * than a 4 digit number.
         * 
         * Example:
         * 
         * Page 389 of file "TestFile.pdf" as jpg would become:
         * 
         * "TestFile0389.jpg".
         * 
         * 
         * There is error correction to verify that:
         * 
         * pageStart is not less than 0, or greater than the last page or pageEnd.
         * pageEnd is not less than 0, less than pageStart or greater than the last page in the PDF document.
         * 
         * 
         * Parameters:
         * 
         * PDFFileName:     String, name of PDF file to be read (does not verify that the file exists).
         * format:          String, default "jpg", format to save images as
         * pageStart:       System.Int32, default 0, page to start from (with starting page as 0).
         * pageEnd:         System.Int32, default -1, last page to print.  -1 means "print to the last page."
         * 
         * Please note that starting page is 0, so a file with 500 pages the pageEnd would be 499.
         * 
         * Returns:
         * 
         *  0 (failure)
         *  1 (success)
         * 
         * */
        public int ConvertPDF2Image(string PDFFileName, String format = "jpg", System.Int32 pageStart = 0, System.Int32 pageEnd = -1)
        {
            try
            {
                // load the PDF document
                System.Int32 index = pdfXpress.Documents.Add(PDFFileName);


                // initialize the variables we need to write out the files
                System.String outputFileBase = System.IO.Path.GetFileNameWithoutExtension(PDFFileName);


                // Please note: If the DIB is being passed to an ImagXpress control
                // then the ProduceDibSection property of the RenderOptions class should be set = False.
                RenderOptions renderOpts = new RenderOptions();
                renderOpts.ProduceDibSection = false;
                renderOpts.ResolutionX = 300;
                renderOpts.ResolutionY = 300;


                //if they give us -1 as the parameter, do all the pages
                if (pageEnd == -1)
                {
                    pageEnd = pdfXpress.Documents[index].PageCount - 1;
                }


                /*
                 * Error checking
                 * 
                 * These could be converted to exceptions in the future
                 * */


                //starting page can't be less than 0, or greater than total pages
                if (pageStart < 0 || pageStart > pdfXpress.Documents[index].PageCount - 1 || pageStart > pageEnd)
                {
                    System.Console.WriteLine("Invalid starting page.  Must be greater than 0 and less than total page count.");
                    return 0;
                }






                //ending page can not be less than the start page or more than the total number of pages
                if (pageEnd < pageStart || pageEnd > pdfXpress.Documents[index].PageCount - 1)
                {


                    System.Console.WriteLine("Invalid end page.  Must be more than the starting page and not greater than the total number of pages.");
                    return 0;
                }


                //allowed image types
                //convert everything to lower case first
                System.Drawing.Imaging.ImageFormat imageType;
                switch (format.ToLower())
                {
                    case "bmp":
                        imageType = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;


                    case "png":
                        imageType = System.Drawing.Imaging.ImageFormat.Png;
                        break;


                    case "gif":
                        imageType = System.Drawing.Imaging.ImageFormat.Gif;
                        break;






                    //if it's unrecognized, just do JPEG
                    case "jpg":
                    case "jpeg":
                    default:
                        imageType = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                }










                System.Int32 totalPages = pageEnd + 1 - pageStart;


                //setting up the string format for leading zeros before any number less than 4 digits long
                String numberFormat = "0000";






                //for (pageIndex = 0; pageIndex < pdfXpress.Documents[index].PageCount; pageIndex++)
                for (int pageIndex = 0; pageStart <= pageEnd; pageStart++)
                {


                    using (Bitmap bp = pdfXpress.Documents[index].RenderPageToBitmap(pageStart, renderOpts))
                    {
                        bp.Save(outputFileBase + pageStart.ToString(numberFormat) + "." + format, imageType);
                    }


                    //show current progress
                    Progress(++pageIndex, totalPages);
                    //ending the progress line


                }
                System.Console.WriteLine();
                //properly dispose of our PDF object
                if (pdfXpress != null)
                {
                    pdfXpress.Dispose();
                    pdfXpress = null;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }




            //success - return 1
            return 1;
        }


        /*
         *  Main:
         *  
         *  Using Accusoft PDFXpress, reads from a PDF document, then converts each page into a bitmap file.
         *  
         *  Saves files are based on the PDF file base name with a 4 digit number and extension as the file format.
         *  
         *  For example, saving page 5 of the PDF file "TestFile.pdf" as a jpg will create the file:
         *  
         *  "TestFile0005.jpg".
         *  
         *  This program takes at least 1, up to 4 parameters:
         *  
         *  args[0]:    (Required) PDF File name to convert to images.  Verifies the file exists, but does not 
         *              verify it is a PDF file before conversion.
         *  args[1]:    File format.  Allowed format are bmp, gif, png, jpg.  Anything else the program will default
         *              to jpg.
         *  args[2]:    Page to start from (0 is the first page)  
         *  args[3]:    Page to end at (since 0 is the first page, this will be lastpage-1, so a 500 page PDF file the 
         *              last page is 499, for example).
         *  
         * 
         */


        static void Main(string[] args)
        {






            System.Console.WriteLine("Welcome to the Accusoft PDF to Image Conversion Demo.");


            //check to make sure we have two arguments - file 1 and file 2
            if (args.Length != 0)
            {
                //The variables used with default values
                String inputFile;
                String format = "jpg";
                System.Int32 pageStart = 0;
                System.Int32 pageEnd = -1;




                //make sure that the files exist
                if (File.Exists(args[0]))
                {
                    //file name
                    inputFile = args[0];


                    //if they provided a format parameter
                    if (args.ElementAtOrDefault(1) != null)
                    {
                        format = args[1];
                    }


                    //if they provided a start page parameter
                    if (args.ElementAtOrDefault(2) != null)
                    {


                        if (!(Int32.TryParse(args[2], out pageStart)))
                        {
                            System.Console.WriteLine(args[2] + " is not a valid number.");
                        }
                    }


                    //if they provided an end page parameter
                    if (args.ElementAtOrDefault(3) != null)
                    {


                        if (!(Int32.TryParse(args[3], out pageEnd)))
                        {
                            System.Console.WriteLine(args[3] + " is not a valid number.");
                        }
                    }








                    System.Console.WriteLine(args[0] + " exists.");
                    PDFXpressConverter newConverter = new PDFXpressConverter();
                    newConverter.ConvertPDF2Image(inputFile, format, pageStart, pageEnd);


                }
                else
                {
                    System.Console.WriteLine("File does not exist: " + args[0]);
                }
            }
            else
            {
                System.Console.WriteLine("PDFXpressConvert program parameters: ");
                System.Console.WriteLine("Usage:  \"PDFXpressConvertPDF2JPG [FileName] [(Optional) Format] [(Optional)Start Page] [(Optional) End Page]\".");
                System.Console.WriteLine("Entering End Page as -1 converts all pages.");
            }










            //requests the user hit enter to end the program
            Console.WriteLine("Hit Enter to terminate.");
            Console.ReadLine();
        }
    }
}