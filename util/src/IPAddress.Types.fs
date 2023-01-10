namespace Util.Types

module IPAddress =
    type IPV4Address = IPV4Address of string

    type InvalidIPV4Address = InvalidIPV4Address of IPV4Address
    type ValidIPV4Address = ValidIPV4Address of IPV4Address

    type PublicIP =
        | Invalid of InvalidIPV4Address
        | Valid of ValidIPV4Address

    type InvalidateIPV4Address = ValidIPV4Address -> InvalidIPV4Address

    type ValidateIPV4Address = InvalidIPV4Address -> ValidIPV4Address
