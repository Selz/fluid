using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Fluid.Ast
{
    public class CaseStatement : TagStatement
    {
        public CaseStatement(
            Expression expression,
            ElseStatement elseStatement = null,
            IList<WhenStatement> whenStatements = null
            ) :base (new List<Statement>())
        {
            Expression = expression;
            Else = elseStatement;
            Whens = whenStatements;
        }

        public Expression Expression { get; }

        public ElseStatement Else { get; }

        public IList<WhenStatement> Whens { get; } = new List<WhenStatement>();

        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            context.IncrementSteps();

            var value = await Expression.EvaluateAsync(context);
            bool hasAnyMatch = false;

            if (Whens != null)
            {
                foreach (var when in Whens)
                {
                    foreach(var option in when.Options)
                    {
                        if (value.Equals(await option.EvaluateAsync(context)))
                        {
                            hasAnyMatch = true;
                            await when.WriteToAsync(writer, encoder, context);
                            break;
                        }
                    }
                }
            }

            if (hasAnyMatch)
            {
                return Completion.Normal;
            }

            if (Else != null)
            {
              await Else.WriteToAsync(writer, encoder, context);
            }

            return Completion.Normal;
        }
    }
}
