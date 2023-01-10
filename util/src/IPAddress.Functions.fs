namespace Util.Functions

module IPAddress = 
  open Util.Types.IPAddress

  let InvalidateIPV4Address : InvalidateIPV4Address = 
    fun valid ->
      let (ValidIPV4Address addr) = valid

      InvalidIPV4Address addr
