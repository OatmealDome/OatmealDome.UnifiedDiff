using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace OatmealDome.UnifiedDiff;

public sealed class UnifiedDiffFile
{
    // The maximum amount of DiffPieces to queue before giving up on merging the current hunk with the next.
    private const int AdjacentPieceMergeCount = 3;
    
    public string AlphaPath
    {
        get;
        set;
    }

    public string BravoPath
    {
        get;
        set;
    }
    
    // TODO: Support for timestamps

    public List<DiffHunk> Hunks
    {
        get;
        set;
    }
    
    public UnifiedDiffFile()
    {
        AlphaPath = "alpha";
        BravoPath = "bravo";
        Hunks = new List<DiffHunk>();
    }

    public static UnifiedDiffFile Create(string alphaStr, string bravoStr, bool ignoreWhiteSpace = true,
        bool ignoreCase = false)
    {
        DiffPaneModel diff = InlineDiffBuilder.Diff(alphaStr, bravoStr, ignoreWhiteSpace, ignoreCase);

        int lineAlpha = 1;
        int lineBravo = 1;
        
        List<DiffPiece> queuedPieces = new List<DiffPiece>();
        DiffHunk? currentHunk = null;
        
        UnifiedDiffFile unifiedDiffFile = new UnifiedDiffFile();
        
        // Create hunks from the DiffPieces, while attempting to merge adjacent hunks.
        foreach (DiffPiece piece in diff.Lines)
        {
            switch (piece.Type)
            {
                case ChangeType.Inserted:
                case ChangeType.Deleted:
                    if (currentHunk == null)
                    {
                        currentHunk = new DiffHunk(lineAlpha, lineBravo);
                    }

                    if (queuedPieces.Count > 0)
                    {
                        foreach (DiffPiece queuedPiece in queuedPieces)
                        {
                            currentHunk.AddDiffPiece(queuedPiece);
                        }
                        
                        queuedPieces.Clear();
                    }

                    if (piece.Type == ChangeType.Inserted)
                    {
                        lineBravo++;
                    }
                    else
                    {
                        lineAlpha++;
                    }
                    
                    currentHunk.AddDiffPiece(piece);
                    
                    break;
                case ChangeType.Unchanged:
                    lineAlpha++;
                    lineBravo++;

                    if (currentHunk != null)
                    {
                        queuedPieces.Add(piece);
                        
                        if (queuedPieces.Count > AdjacentPieceMergeCount)
                        {
                            queuedPieces.Clear();

                            unifiedDiffFile.Hunks.Add(currentHunk);
                            currentHunk = null;
                        }
                    }

                    break;
            }
        }
        
        // Push back the last hunk if it hasn't been already
        if (currentHunk != null)
        {
            unifiedDiffFile.Hunks.Add(currentHunk);
        }
        
        // Add pre and post context lines to each hunk
        foreach (DiffHunk thisHunk in unifiedDiffFile.Hunks)
        {
            int bravoStart = thisHunk.BravoStart;
            int bravoLength = thisHunk.BravoLength;

            for (int i = 0; i < 3; i++)
            {
                int contextLine = bravoStart - i - 1;

                DiffPiece? piece = diff.Lines.FirstOrDefault(x => x.Position == contextLine);
                if (piece == null)
                {
                    break;
                }

                thisHunk.AddContextPiece(piece, false);
            }

            for (int i = 0; i < 3; i++)
            {
                int contextLine = bravoStart + bravoLength + i;

                DiffPiece? piece = diff.Lines.FirstOrDefault(x => x.Position == contextLine);
                if (piece == null)
                {
                    break;
                }

                thisHunk.AddContextPiece(piece, true);
            }
        }

        return unifiedDiffFile;
    }

    public static UnifiedDiffFile Create(Stream alphaStream, Stream bravoStream, bool ignoreWhiteSpace = true,
        bool ignoreCase = false)
    {
        using StreamReader alphaReader = new StreamReader(alphaStream);
        using StreamReader bravoReader = new StreamReader(bravoStream);

        string alphaStr = alphaReader.ReadToEnd();
        string bravoStr = bravoReader.ReadToEnd();

        return Create(alphaStr, bravoStr, ignoreWhiteSpace, ignoreCase);
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        
        // Header
        builder.AppendLine($"--- {AlphaPath}");
        builder.AppendLine($"+++ {BravoPath}");
        
        // Hunks
        foreach (DiffHunk hunk in Hunks)
        {
            builder.Append(hunk);
        }

        return builder.ToString();
    }
}
