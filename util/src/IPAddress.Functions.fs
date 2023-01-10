namespace Util.Functions

module IPAddress =
    open Util.Types.IPAddress

    let ValidateIPV4Address: ValidateIPV4Address =
        fun unvalidated ->
            let (InvalidIPV4Address invalid) = unvalidated
            ValidIPV4Address invalid

    let InvalidateIPV4Address: InvalidateIPV4Address =
        fun valid ->
            let (ValidIPV4Address addr) = valid

            InvalidIPV4Address addr
