﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using MySqlParser;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

namespace MySql.Data.VisualStudio.LanguageService
{
  internal sealed class MySqlOutliningTagger : ITagger<IOutliningRegionTag>
  {
    string startHide = "[";     //the characters that start the outlining region
    string endHide = "]";       //the characters that end the outlining region
    string ellipsis = "...";    //the characters that are displayed when the region is collapsed
    string hoverText = "hover text"; //the contents of the tooltip for the collapsed span
    ITextBuffer buffer;
    ITextSnapshot snapshot;
    List<Region> regions;

    public MySqlOutliningTagger(ITextBuffer buffer)
    {
      this.buffer = buffer;
      this.snapshot = buffer.CurrentSnapshot;
      this.regions = new List<Region>();
      this.ReParse();
      this.buffer.Changed += BufferChanged;
    }

    public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
      if (spans.Count == 0)
        yield break;
      List<Region> currentRegions = this.regions;
      ITextSnapshot currentSnapshot = this.snapshot;
      SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
      int startLineNumber = entire.Start.GetContainingLine().LineNumber;
      int endLineNumber = entire.End.GetContainingLine().LineNumber;
      foreach (var region in currentRegions)
      {
        if (region.StartLine <= endLineNumber &&
            region.EndLine >= startLineNumber)
        {
          var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
          var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

          //the region starts at the beginning of the "[", and goes until the *end* of the line that contains the "]".
          yield return new TagSpan<IOutliningRegionTag>(
              new SnapshotSpan(startLine.Start + region.StartOffset,
              endLine.End),
              new OutliningRegionTag(false, false, ellipsis, hoverText));
        }
      }
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    void BufferChanged(object sender, TextContentChangedEventArgs e)
    {
      // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
      if (e.After != buffer.CurrentSnapshot)
        return;
      this.ReParse();
    }

    void ReParse()
    {
      ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
      List<Region> newRegions = new List<Region>();

      string sql = newSnapshot.GetText();
      StringBuilder sb = new StringBuilder();
      MySQL51Parser.statement_list_return r = LanguageServiceUtil.ParseSql(sql, false, out sb);

      if (sb.Length != 0) return;

      CommonTree ct = (CommonTree)r.Tree;
      foreach (ITree c in ct.Children)
      {
        newRegions.Add(new Region()
        {
          Level = 1,
          StartLine = c.Line,
          StartOffset = c.TokenStartIndex,
          EndLine = -1,
          EndOffset = c.TokenStopIndex,
          PartialParent = null
        });
      }

      ITextSnapshot snap = this.snapshot;      
      if (this.TagsChanged != null)
        this.TagsChanged(this, new SnapshotSpanEventArgs(
            new SnapshotSpan(this.snapshot, Span.FromBounds( 0, snap.Length ))));
    }

    void ReParse2()
    {
      ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
      List<Region> newRegions = new List<Region>();

      //keep the current (deepest) partial region, which will have
      // references to any parent partial regions.
      PartialRegion currentRegion = null;

      foreach (var line in newSnapshot.Lines)
      {
        int regionStart = -1;
        string text = line.GetText();

        //lines that contain a "[" denote the start of a new region.
        if ((regionStart = text.IndexOf(startHide, StringComparison.Ordinal)) != -1)
        {
          int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
          int newLevel;
          if (!TryGetLevel(text, regionStart, out newLevel))
            newLevel = currentLevel + 1;

          //levels are the same and we have an existing region;
          //end the current region and start the next
          if (currentLevel == newLevel && currentRegion != null)
          {
            newRegions.Add(new Region()
            {
              Level = currentRegion.Level,
              StartLine = currentRegion.StartLine,
              StartOffset = currentRegion.StartOffset,
              EndLine = line.LineNumber
            });

            currentRegion = new PartialRegion()
            {
              Level = newLevel,
              StartLine = line.LineNumber,
              StartOffset = regionStart,
              PartialParent = currentRegion.PartialParent
            };
          }
          //this is a new (sub)region
          else
          {
            currentRegion = new PartialRegion()
            {
              Level = newLevel,
              StartLine = line.LineNumber,
              StartOffset = regionStart,
              PartialParent = currentRegion
            };
          }
        }
        //lines that contain "]" denote the end of a region
        else if ((regionStart = text.IndexOf(endHide, StringComparison.Ordinal)) != -1)
        {
          int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
          int closingLevel;
          if (!TryGetLevel(text, regionStart, out closingLevel))
            closingLevel = currentLevel;

          //the regions match
          if (currentRegion != null &&
              currentLevel == closingLevel)
          {
            newRegions.Add(new Region()
            {
              Level = currentLevel,
              StartLine = currentRegion.StartLine,
              StartOffset = currentRegion.StartOffset,
              EndLine = line.LineNumber
            });

            currentRegion = currentRegion.PartialParent;
          }
        }
      }

      //determine the changed span, and send a changed event with the new spans
      List<Span> oldSpans =
          new List<Span>(this.regions.Select(r => AsSnapshotSpan(r, this.snapshot)
              .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
              .Span));
      List<Span> newSpans =
              new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

      NormalizedSpanCollection oldSpanCollection = new NormalizedSpanCollection(oldSpans);
      NormalizedSpanCollection newSpanCollection = new NormalizedSpanCollection(newSpans);

      //the changed regions are regions that appear in one set or the other, but not both.
      NormalizedSpanCollection removed =
      NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

      int changeStart = int.MaxValue;
      int changeEnd = -1;

      if (removed.Count > 0)
      {
        changeStart = removed[0].Start;
        changeEnd = removed[removed.Count - 1].End;
      }

      if (newSpans.Count > 0)
      {
        changeStart = Math.Min(changeStart, newSpans[0].Start);
        changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
      }

      this.snapshot = newSnapshot;
      this.regions = newRegions;

      if (changeStart <= changeEnd)
      {
        ITextSnapshot snap = this.snapshot;
        if (this.TagsChanged != null)
          this.TagsChanged(this, new SnapshotSpanEventArgs(
              new SnapshotSpan(this.snapshot, Span.FromBounds(changeStart, changeEnd))));
      }
    }

    static bool TryGetLevel(string text, int startIndex, out int level)
    {
      level = -1;
      if (text.Length > startIndex + 3)
      {
        if (int.TryParse(text.Substring(startIndex + 1), out level))
          return true;
      }

      return false;
    }

    static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
    {
      var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
      var endLine = (region.StartLine == region.EndLine) ? startLine
           : snapshot.GetLineFromLineNumber(region.EndLine);
      return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
    }
  }

  internal class PartialRegion
  {
    internal int StartLine { get; set; }
    internal int StartOffset { get; set; }
    internal int Level { get; set; }
    internal PartialRegion PartialParent { get; set; }
  }

  internal class Region : PartialRegion
  {
    protected internal int EndLine { get; set; }
    protected internal int EndOffset { get; set; }
  } 
}
