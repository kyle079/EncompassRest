using System;
using System.ComponentModel;
using EncompassRest.Utilities;

namespace EncompassRest.Loans
{
    public sealed class LoanFields
    {
        [Obsolete("Use LoanFieldDescriptors.FieldMappings instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LoanFieldMappings FieldMappings => LoanFieldDescriptors.FieldMappings;

        [Obsolete("Use LoanFieldDescriptors.FieldPatternMappings instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LoanFieldPatternMappings FieldPatternMappings => LoanFieldDescriptors.FieldPatternMappings;

        private readonly Loan _loan;

        public LoanField this[string fieldId]
        {
            get
            {
                Preconditions.NotNullOrEmpty(fieldId, nameof(fieldId));

                FieldDescriptor descriptor;
                int? borrowerPairIndex = null;
                ModelPath modelPath = null;
                if (fieldId.Length >= 2 && fieldId[fieldId.Length - 2] == '#')
                {
                    borrowerPairIndex = fieldId[fieldId.Length - 1] - '1';
                    if (borrowerPairIndex < 0 || borrowerPairIndex > 5)
                    {
                        throw new ArgumentException($"Could not find field '{fieldId}'");
                    }

                    var strippedFieldId = fieldId.Substring(0, fieldId.Length - 2);

                    descriptor = LoanFieldDescriptors.GetFieldDescriptor(strippedFieldId, _loan.Client?.Loans.FieldDescriptors.CustomFields);

                    var path = descriptor.ModelPath;
                    if (!path.StartsWith("Loan.CurrentApplication.", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"Could not find field '{fieldId}'");
                    }

                    modelPath = LoanFieldDescriptors.CreateModelPath($"Loan.Applications[(ApplicationIndex == '{borrowerPairIndex}')]{path.Substring(23)}");
                }
                else
                {
                    descriptor = LoanFieldDescriptors.GetFieldDescriptor(fieldId, _loan.Client?.Loans.FieldDescriptors.CustomFields);
                }

                switch (descriptor.Type)
                {
                    case LoanFieldType.Custom:
                        return new CustomLoanField(descriptor, _loan);
                    case LoanFieldType.Virtual:
                        return new VirtualLoanField(descriptor, _loan);
                    default:
                        return new LoanField(descriptor, _loan, modelPath, borrowerPairIndex);
                }
            }
        }

        internal LoanFields(Loan loan)
        {
            _loan = loan;
        }
    }
}