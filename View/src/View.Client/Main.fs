module View.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Microsoft.JSInterop
open Microsoft.AspNetCore.Components
open System.Web
open System.Text.Json
open System.Text.Json.Serialization


module CandlesService = 
    let private toQueryString (queryParams: (string*string) list) =
        let list = queryParams
                   |> List.map (fun (key, value) -> String.Format("{0}={1}", 
                                                                   HttpUtility.UrlEncode(key), 
                                                                   HttpUtility.UrlEncode(value)))
        "?" + String.Join("&", list)

    let createGetCandlesUri symbol (periodSeconds:int) 
                                    (_startTime: int64 option) (_endTime: int64 option) (_limit: int option) =
        let startTime = defaultArg _startTime 0L
        let endTime = defaultArg _endTime 0L
        let limit = defaultArg _limit 0
        let queryParams = ["symbol", symbol; "periodSeconds", periodSeconds.ToString(); "startTime", startTime.ToString();
                           "endTime", endTime.ToString(); "limit", limit.ToString()]
        "https://localhost:5001/api/candles" + toQueryString(queryParams)

[<Inject>]
let JSRuntime = Unchecked.defaultof<IJSRuntime>

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data
    | [<EndPoint "/chart">] Chart

/// The Elmish application's model.
type Model =
    {
        page: Page
        counter: int
        books: Book[] option
        error: string option
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
        pairId: string
        interval: int
        limit: int
        startTime: DateTimeOffset
        endTime: DateTimeOffset
    }

and Book =
    {
        title: string
        author: string
        publishDate: DateTime
        isbn: string
    }

let initModel =
    {
        page = Home
        counter = 0
        books = None
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
        pairId = "0x8ad599c3a0ff1de082011efddc58f1908eb6e6d8"
        interval = 300
        limit = 10
        startTime = DateTimeOffset.Now - TimeSpan.FromDays(30.0)
        endTime = DateTimeOffset.Now
    }

/// Remote service definition.
type BookService =
    {
        /// Get the list of all books in the collection.
        getBooks: unit -> Async<Book[]>

        /// Add a book in the collection.
        addBook: Book -> Async<unit>

        /// Remove a book from the collection, identified by its ISBN.
        removeBookByIsbn: string -> Async<unit>

        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Increment
    | Decrement
    | SetCounter of int
    | GetBooks
    | GotBooks of Book[]
    | SetUsername of string
    | SetPassword of string
    | SetPairId of string
    | SetInterval of int
    | SetLimit of int
    | SetStartTime of DateTimeOffset
    | SetEndTime of DateTimeOffset
    | DrawChart
    | DrawChartError
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError
    |SuccessfulDraw

let update (js: IJSRuntime) remote message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetBooks
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.none

    | GetBooks ->
        let cmd = Cmd.OfAsync.either remote.getBooks () GotBooks Error
        { model with books = None }, cmd
    | GotBooks books ->
        { model with books = Some books }, Cmd.none

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none

    | SetPairId newPairId ->
        {model with pairId = newPairId}, Cmd.none
    | SetInterval newInterval ->
        {model with interval = newInterval}, Cmd.none
    | SetLimit newLimit ->
        {model with limit = newLimit}, Cmd.none
    | SetStartTime newStartTime ->
        {model with startTime = newStartTime}, Cmd.none
    | SetEndTime newEndTime ->
        {model with endTime = newEndTime}, Cmd.none

    | DrawChart ->
        let startTime = model.startTime.ToUnixTimeSeconds() |> Some
        let endTime = model.endTime.ToUnixTimeSeconds() |> Some
        let limit = model.limit |> Some
        let uri = CandlesService.createGetCandlesUri model.pairId model.interval startTime endTime limit
        let func = fun () -> js.InvokeVoidAsync("drawChart", uri).AsTask() |> Async.AwaitTask
        model,
        Cmd.OfAsync.either func () (fun () -> SuccessfulDraw) Error//Error
    
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let counterPage model dispatch =
    Main.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
        .Elt()

let dataPage model (username: string) dispatch =
    Main.Data()
        .Reload(fun _ -> dispatch GetBooks)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
        .Rows(cond model.books <| function
            | None ->
                Main.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr [] [
                        td [] [text book.title]
                        td [] [text book.author]
                        td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
                        td [] [text book.isbn]
                    ])
        .Elt()

let chartPage model dispatch =
    Main.Chart()
        .PairId(model.pairId, fun pairId -> dispatch (SetPairId pairId))
        .Interval(model.interval, fun interval -> dispatch (SetInterval interval))
        .Limit(model.limit, fun limit -> dispatch (SetLimit limit))
        .StartTime(model.startTime.ToString("s"), fun startTime -> dispatch (DateTimeOffset.Parse(startTime) |> SetStartTime))
        .EndTime(model.endTime.ToString("s"), fun endTime -> dispatch (DateTimeOffset.Parse(endTime) |> SetEndTime))
        .DrawChart(fun _ -> dispatch DrawChart)
        .Elt()

let signInPage model dispatch =
    Main.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty
            | true ->
                Main.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Counter "Counter"
            menuItem model Data "Download data"
            menuItem model Chart "Chart"
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | Data ->
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
            | Chart -> chartPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let bookService = this.Remote<BookService>()
        let update = update this.JSRuntime bookService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
