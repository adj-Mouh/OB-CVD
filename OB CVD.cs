#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CumulativeBboDelta : Indicator
	{
		#region Variables
		private List<DOMRow> askRows;
		private List<DOMRow> bidRows;
		
		private long cumulativeDelta;
		private long lastBestBidVol;
		private long lastBestAskVol;
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Calculates a cumulative delta based on the volume changes at the best bid and best ask prices.";
				Name										= "Cumulative BBO Delta";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				// Add the plot for our cumulative delta line
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "CumulativeDelta");
				// Add a zero line for reference
				AddLine(Brushes.Gray, 0, "ZeroLine");
			}
			else if (State == State.DataLoaded)
			{
				// Initialize lists and variables
				askRows 		= new List<DOMRow>();
				bidRows 		= new List<DOMRow>();
				cumulativeDelta = 0;
				lastBestBidVol 	= 0;
				lastBestAskVol 	= 0;
			}
		}
		
		protected override void OnBarUpdate()
		{
			// On a new bar, carry over the last calculated delta value
			// to prevent the line from dropping to zero until the next market depth update.
			if (CurrentBar > 0)
			{
				Value[0] = Value[1];
			}
		}
		
		protected override void OnMarketDepth(MarketDepthEventArgs e)
		{
			// This method is called on every Level 2 order book change.
			List<DOMRow> oneDOMRow = null;

			// Determine if the update is for the Ask or Bid side
			if (e.MarketDataType == MarketDataType.Ask)
				oneDOMRow = askRows;
			else if (e.MarketDataType == MarketDataType.Bid)
				oneDOMRow = bidRows;
			
			if (oneDOMRow == null)
				return;
			
			// Lock the list to ensure thread safety while modifying it
			lock (oneDOMRow)
			{
				if (e.Operation == Operation.Add)
					oneDOMRow.Insert(e.Position, new DOMRow(e.Price, e.Volume));
				
				else if (e.Operation == Operation.Remove && e.Position < oneDOMRow.Count)
					oneDOMRow.RemoveAt(e.Position);
				
				else if (e.Operation == Operation.Update && e.Position < oneDOMRow.Count)
				{
					oneDOMRow[e.Position].Price = e.Price;
					oneDOMRow[e.Position].Volume = e.Volume;
				}
			}

			// --- Core Delta Calculation Logic ---
			
			long currentBestBidVol = 0;
			long currentBestAskVol = 0;

			// Safely get the current volume at the best bid (highest bid price)
			lock(bidRows)
			{
				if (bidRows.Count > 0)
					currentBestBidVol = bidRows[0].Volume;
			}

			// Safely get the current volume at the best ask (lowest ask price)
			lock(askRows)
			{
				if (askRows.Count > 0)
					currentBestAskVol = askRows[0].Volume;
			}
			
			// If we don't have both sides of the book, we can't calculate a delta
			if (currentBestBidVol == 0 || currentBestAskVol == 0)
				return;
			
			// Calculate the change in volume since the last update
			long bidChange = currentBestBidVol - lastBestBidVol;
			long askChange = currentBestAskVol - lastBestAskVol;
			
			// Update cumulative delta based on the logic: (Sell Limit Volume) - (Buy Limit Volume)
			// An increase in ask volume increases the delta.
			// An increase in bid volume decreases the delta.
			cumulativeDelta += (askChange - bidChange);
			
			// Store the current volumes for the next calculation
			lastBestBidVol = currentBestBidVol;
			lastBestAskVol = currentBestAskVol;

			// Set the plot value and force the chart to redraw
			Value[0] = cumulativeDelta;
			ForceRefresh();
		}
		
		#region Helper Class
		// A simple class to hold Price and Volume for a single row in the order book
		private class DOMRow
		{
			public double Price;
			public long Volume;

			public DOMRow(double myPrice, long myVolume)
			{
				Price = myPrice;
				Volume = myVolume;
			}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CumulativeBboDelta[] cacheCumulativeBboDelta;
		public CumulativeBboDelta CumulativeBboDelta()
		{
			return CumulativeBboDelta(Input);
		}

		public CumulativeBboDelta CumulativeBboDelta(ISeries<double> input)
		{
			if (cacheCumulativeBboDelta != null)
				for (int idx = 0; idx < cacheCumulativeBboDelta.Length; idx++)
					if (cacheCumulativeBboDelta[idx] != null &&  cacheCumulativeBboDelta[idx].EqualsInput(input))
						return cacheCumulativeBboDelta[idx];
			return CacheIndicator<CumulativeBboDelta>(new CumulativeBboDelta(), input, ref cacheCumulativeBboDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CumulativeBboDelta CumulativeBboDelta()
		{
			return indicator.CumulativeBboDelta(Input);
		}

		public Indicators.CumulativeBboDelta CumulativeBboDelta(ISeries<double> input )
		{
			return indicator.CumulativeBboDelta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CumulativeBboDelta CumulativeBboDelta()
		{
			return indicator.CumulativeBboDelta(Input);
		}

		public Indicators.CumulativeBboDelta CumulativeBboDelta(ISeries<double> input )
		{
			return indicator.CumulativeBboDelta(input);
		}
	}
}

#endregion
