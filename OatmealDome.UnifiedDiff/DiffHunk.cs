using System.Text;
using DiffPlex.DiffBuilder.Model;

namespace OatmealDome.UnifiedDiff;

public sealed class DiffHunk
{
    public int AlphaStart
    {
        get;
        private set;
    }

    public int BravoStart
    {
        get;
        private set;
    }

    public int AlphaLength
    {
        get;
        private set;
    }

    public int BravoLength
    {
        get;
        private set;
    }

    private List<string> _lines;

    public DiffHunk(int alphaStart, int bravoStart)
    {
        AlphaStart = alphaStart;
        BravoStart = bravoStart;
        AlphaLength = 0;
        BravoLength = 0;
        
        _lines = new List<string>();
    }

    public void AddDiffPiece(DiffPiece piece)
    {
        switch (piece.Type)
        {
            case ChangeType.Deleted:
                _lines.Add($"-{piece.Text}");
                
                AlphaLength++;
                
                break;
            case ChangeType.Inserted:
                _lines.Add($"+{piece.Text}");
                
                BravoLength++;
                
                break;
            case ChangeType.Unchanged:
                _lines.Add($" {piece.Text}");
                
                AlphaLength++;
                BravoLength++;
                
                break;
            default:
                throw new ArgumentException("Unsupported ChangeType in DiffPiece");
        }
    }

    public void AddContextPiece(DiffPiece piece, bool post)
    {
        string formattedLine = $" {piece.Text}";
        
        if (post)
        {
            _lines.Add(formattedLine);
        }
        else
        {
            _lines.Insert(0, formattedLine);
            
            AlphaStart--;
            BravoStart--;
        }
        
        AlphaLength++;
        BravoLength++;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        
        // Header
        builder.AppendLine($"@@ -{AlphaStart},{AlphaLength} +{BravoStart},{BravoLength} @@");
        
        // Actual lines
        foreach (string line in _lines)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }
}