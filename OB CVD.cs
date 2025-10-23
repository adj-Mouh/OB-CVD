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
	public class CumulativeL2Delta : Indicator
	{
		#region Variables
		private double cumulativeDelta;
		private long bestBidVolume;
		private long bestAskVolume;
		private object dataLock = new object(); // For thread safety
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Calculates a cumulative delta of the best bid/ask volume, resetting each bar.";
				Name										= "Cumulative L2 Delta";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;

				// Add the plot for our cumulative delta line
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "CumL2Delta");
			}
			else if (State == State.DataLoaded)
			{
				// Initialize variables
				cumulativeDelta = 0;
				bestBidVolume = 0;
				bestAskVolume = 0;
			}
		}

		protected override void OnBarUpdate()
		{
			// Detect the first tick of a new bar
			if (IsFirstTickOfBar)
			{
				// Reset the cumulative delta value to 0 for the new bar
				cumulativeDelta = 0;
				
				// Optional: You could also reset bestBidVolume and bestAskVolume here if you want
				// a clean slate, but it's not strictly necessary as they'll be updated by OnMarketDepth.
				// bestBidVolume = 0;
				// bestAskVolume = 0;
			}
			
			// Set the plot value for the current bar.
			// This will update on every tick, showing the latest cumulative value.
			Value[0] = cumulativeDelta;
		}
		
		protected override void OnMarketDepth(MarketDepthEventArgs e)
		{
			// We only care about changes to the very top of the book (best bid/ask)
			if (e.Position != 0)
				return;
				
			// Lock to prevent race conditions when accessing shared variables from different threads
			lock(dataLock)
			{
				bool topLevelChanged = false;
				
				// Check if the best bid volume has changed
				if (e.MarketDataType == MarketDataType.Bid)
				{
					if (bestBidVolume != e.Volume)
					{
						bestBidVolume = e.Volume;
						topLevelChanged = true;
					}
				}
				// Check if the best ask volume has changed
				else if (e.MarketDataType == MarketDataType.Ask)
				{
					if (bestAskVolume != e.Volume)
					{
						bestAskVolume = e.Volume;
						topLevelChanged = true;
					}
				}

				// If a change occurred and we have valid data for both sides, perform the calculation
				if (topLevelChanged && bestBidVolume > 0 && bestAskVolume > 0)
				{
					// Calculate the raw delta for this specific event
					long rawDelta = bestBidVolume - bestAskVolume;
					
					// Add this raw delta to our cumulative total for the current bar
					cumulativeDelta += rawDelta;
					
					// Update the plot value immediately
					Value[0] = cumulativeDelta;
					
					// Force the chart to repaint to show the latest value instantly.
					// This is needed because we are updating the plot from an event handler
					// that is not OnBarUpdate().
					ForceRefresh();
				}
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CumulativeL2Delta[] cacheCumulativeL2Delta;
		public CumulativeL2Delta CumulativeL2Delta()
		{
			return CumulativeL2Delta(Input);
		}

		public CumulativeL2Delta CumulativeL2Delta(ISeries<double> input)
		{
			if (cacheCumulativeL2Delta != null)
				for (int idx = 0; idx < cacheCumulativeL2Delta.Length; idx++)
					if (cacheCumulativeL2Delta[idx] != null &&  cacheCumulativeL2Delta[idx].EqualsInput(input))
						return cacheCumulativeL2Delta[idx];
			return CacheIndicator<CumulativeL2Delta>(new CumulativeL2Delta(), input, ref cacheCumulativeL2Delta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CumulativeL2Delta CumulativeL2Delta()
		{
			return indicator.CumulativeL2Delta(Input);
		}

		public Indicators.CumulativeL2Delta CumulativeL2Delta(ISeries<double> input )
		{
			return indicator.CumulativeL2Delta(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CumulativeL2Delta CumulativeL2Delta()
		{
			return indicator.CumulativeL2Delta(Input);
		}

		public Indicators.CumulativeL2Delta CumulativeL2Delta(ISeries<double> input )
		{
			return indicator.CumulativeL2Delta(input);
		}
	}
}

#endregion
