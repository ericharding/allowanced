module FileProviders
open Scriban


type IFileProvider =
  abstract member getTemplate: unit -> Family seq
