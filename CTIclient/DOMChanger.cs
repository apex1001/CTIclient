/*
 * DOMChanger for CTIclient.
 * 
 * @Author: V. Vogelesang
 * 
 * Based on the tutorials @
 * http://stackoverflow.com/questions/5643819/developing-internet-explorer-extensions
 * http://bytes.com/topic/c-sharp/answers/554498-browser-helper-object 
 * 
 * Many thanx to to the authors of these articles :-).
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Timers;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;
using SHDocVw;

namespace CTIclient
{
    public class DOMChanger 
    {
        private BHOController controller;
        private SHDocVw.WebBrowser browserWindow;
        private HTMLDocument document;
        private ElementDispatcher dp;

        ArrayList forbiddenTags = new ArrayList { "script", "style", "img", "audio", "table", "time", "video" };
        String regex = "(0|\\+|\\(\\+|\\(0)[0-9- ()]{9,}";
   
        public DOMChanger(BHOController controller)
        {
            this.controller = controller;
            dp = new ElementDispatcher(controller);
        }

        /**
         * Change the DOM for given explorer window/webbrowser.
         * 
         * @param Explorer tab instance
         * 
         */
        public void changeDOM(SHDocVw.WebBrowser explorer)
        {            
            this.browserWindow = (SHDocVw.WebBrowser)explorer;
            this.document = (HTMLDocument)explorer.Document;

            if (document != null)
            {
                hookEventHandlers();
            }

            HighLightPhoneNumbers();
        }

        /**
          * Handle mouse movement after document change
          * 
          * @param EventObject
          * 
          */
        public void MouseHandler(IHTMLEventObj e)
        {            
            HTMLDocumentEvents2_Event docEvent = (document as HTMLDocumentEvents2_Event);
            docEvent.onmousemove -= new HTMLDocumentEvents2_onmousemoveEventHandler(MouseHandler);
            HighLightPhoneNumbers();
        }

        /**
         * Handle Downloadevent
         * 
         */
        public void DownloadHandler()
        {
            HighLightPhoneNumbers();
        }

        /**
         * Handle ReadyStatechange event    
         * 
         * @param EventObject
         * 
         */
        public void OnReadyStateChangeHandler(IHTMLEventObj e)
        {
            HighLightPhoneNumbers();
        }

        /**
         * Handle PropertyChange event
         * Fires on _dynamic_ changes to the DOM
         * 
         * @param EventObject
         * 
         */
        public void OnPropertyChangeHandler(IHTMLEventObj e)
        {
            HTMLDocumentEvents2_Event docEvent = (document as HTMLDocumentEvents2_Event);
            docEvent.onmousemove += new HTMLDocumentEvents2_onmousemoveEventHandler(MouseHandler);
            HighLightPhoneNumbers();
        }

        /**
         * Handle onload event
         * Fires on _static_ changes to the DOM / document complete
         * 
         *  @param EventObject
         */
        public void OnLoadHandler(IHTMLEventObj e)
        {
            HighLightPhoneNumbers();
        }

        /**
         * Start the highlight process
         * 
         */
        public void HighLightPhoneNumbers()
        {
            IHTMLElementCollection elements = document.body.all;

            foreach (IHTMLElement el in elements)
            {
                if (!forbiddenTags.Contains(el.tagName.ToLower()) && el.id != "tel")
                {
                    IHTMLDOMNode domNode = el as IHTMLDOMNode;
                    if (domNode.hasChildNodes())
                    {
                        IHTMLDOMChildrenCollection domNodeChildren = domNode.childNodes;
                        foreach (IHTMLDOMNode child in domNodeChildren)
                        {
                            if (child.nodeType == 3)
                            {
                                MatchCollection matches = Regex.Matches(child.nodeValue, regex);
                                if (matches.Count > 0)
                                {
                                    String newChildNodeValue = child.nodeValue;
                                    foreach (Match match in matches)
                                    {
                                        String hlText = match.Value;
                                        newChildNodeValue = newChildNodeValue.Replace(hlText,
                                            "<a name=\"tel\" id=\"tel\" href=\"javascript:void(0)\">" + hlText + "</a>");
                                    }
                                    IHTMLElement newChild = document.createElement("text");
                                    newChild.innerHTML = newChildNodeValue;
                                    child.replaceNode((IHTMLDOMNode)newChild);
                                }
                            }
                        }
                    }
                }
            }

            // Get all a elements wit phonenumber and add onclick evenhandler            
            IHTMLElementCollection telElements = document.getElementsByName("tel");
            foreach (IHTMLElement el in telElements)
            {
                IHTMLElement2 el2 = el as IHTMLElement2;
                el2.attachEvent("onclick", dp);
            }   
        }

        /**
         * Hook all eventhandlers to the document
         * 
         * @param explorer tab instance
         * 
         */
        private void hookEventHandlers()
        {
            // Add events for hooking into DHTML DOM events
            HTMLWindowEvents2_Event windowEvent = (document.parentWindow as HTMLWindowEvents2_Event);
            HTMLDocumentEvents2_Event docEvent = (document as HTMLDocumentEvents2_Event);

            // First unhook all event handlers.
            try
            {
                windowEvent.onload -= new HTMLWindowEvents2_onloadEventHandler(OnLoadHandler);
                docEvent.onpropertychange -= new HTMLDocumentEvents2_onpropertychangeEventHandler(OnPropertyChangeHandler);
                docEvent.onreadystatechange -= new HTMLDocumentEvents2_onreadystatechangeEventHandler(OnReadyStateChangeHandler);
                browserWindow.DownloadBegin -= new DWebBrowserEvents2_DownloadBeginEventHandler(DownloadHandler);
                docEvent.onmousemove -= new HTMLDocumentEvents2_onmousemoveEventHandler(MouseHandler);
            }
            catch { }

            // Now hook them all, except mouseOver.
            windowEvent.onload += new HTMLWindowEvents2_onloadEventHandler(OnLoadHandler);
            docEvent.onpropertychange += new HTMLDocumentEvents2_onpropertychangeEventHandler(OnPropertyChangeHandler);
            docEvent.onreadystatechange += new HTMLDocumentEvents2_onreadystatechangeEventHandler(OnReadyStateChangeHandler);
            browserWindow.DownloadBegin += new DWebBrowserEvents2_DownloadBeginEventHandler(DownloadHandler);
        }

        /**
         * Dispatcher class for element onclick event
         * 
         */
        public class ElementDispatcher
        {
            private bool elementClicked;
            private System.Timers.Timer timer;
            BHOController controller;

            public ElementDispatcher(BHOController controller)
            {
                this.controller = controller;
                elementClicked = false;
                timer = new System.Timers.Timer(300);
                timer.Elapsed += new ElapsedEventHandler(pauseElementClick);
            }

            /**
              * Catch element click
              * 
              * @param EventObject
              * 
              */
            [DispId(0)]
            public void elementIsClicked(IHTMLEventObj e)
            {
                if (!elementClicked)
                {
                    controller.dial(e.srcElement.innerText); 
                    elementClicked = true;
                    timer.Enabled = true;   
                }
            }

            /**
             * Pause acceptance of element clicks
             * 
             * @param Object source for event
             * @param ??
             * 
             */
            private void pauseElementClick(object source, ElapsedEventArgs e)
            {
                timer.Enabled = false;
                elementClicked = false;
            }
        }
    }
}
