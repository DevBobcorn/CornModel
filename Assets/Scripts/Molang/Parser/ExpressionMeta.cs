using System.Text;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser
{
    /// <summary>
    ///		Contains metadata about an expression
    /// </summary>
    public class ExpressionMeta
    {
        /// <summary>
        ///     The token
        /// </summary>
        public Token Token { get; set; }
        
        /// <summary>
        ///     The parent expression
        /// </summary>
        public IExpression Parent { get; set; }
        
        /// <summary>
        ///     The previous expression
        /// </summary>
        public IExpression Previous { get; set; }
        
        /// <summary>
        ///     The next expression
        /// </summary>
        public IExpression Next { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(255);
            bool includeFileInfoIfAvailable;

            if (Token != null)
            {
                sb.Append(Token.Text);
                includeFileInfoIfAvailable = true;
            }
            else
            {
                includeFileInfoIfAvailable = false;
            }

            if (includeFileInfoIfAvailable)
            {
                //	sb.Append(" at offset ");

                //	if (_nativeOffset == OFFSET_UNKNOWN)
                //		sb.Append("<offset unknown>");
                //	else
                //		sb.Append(_nativeOffset);

                sb.Append(" in file:line:column ");
                sb.Append("<filename unknown>");
                sb.Append(':');
                sb.Append(Token.Position.LineNumber);
                sb.Append(':');
                sb.Append(Token.Position.Index);
            }
            else
            {
                sb.Append("<null>");
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}