namespace Domain

type Command<'a> =
    { Data: 'a
      TimeStamp: System.DateTime }
