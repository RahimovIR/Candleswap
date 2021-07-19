module View.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open View

let page = doctypeHtml [] [
    head [] [
        meta [attr.charset "UTF-8"]
        meta [attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0"]
        title [] [text "Bolero Application"]
        ``base`` [attr.href "/"]
        link [attr.rel "stylesheet"; attr.href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.4/css/bulma.min.css"]
        link [attr.rel "stylesheet"; attr.href "css/index.css"]
        link [attr.rel "stylesheet"; attr.href "css/bootstrap/bootstrap.min.css"]
    ]
    body [] [
        nav [attr.classes ["navbar"; "is-dark"]; "role" => "navigation"; attr.aria "label" "main navigation"] [
            div [attr.classes ["navbar-brand"]] [
                a [attr.classes ["navbar-item"; "has-text-weight-bold"; "is-size-5"]; attr.href "https://fsbolero.io"] [
                    img [attr.style "height:40px"; attr.src "https://github.com/fsbolero/website/raw/master/src/Website/img/wasm-fsharp.png"]
                    text "  Bolero"
                ]
            ]
        ]
        div [attr.id "main"] [rootComp<Client.Main.MyApp>]
        script [attr.src "https://unpkg.com/lightweight-charts/dist/lightweight-charts.standalone.production.js"] []
        script [attr.src "js/chart.js"; attr.defer true] []
        (*<script src="https://unpkg.com/lightweight-charts/dist/lightweight-charts.standalone.production.js"></script>
        <script src="js/chart.js" defer></script>*)
        boleroScript
    ]
]
