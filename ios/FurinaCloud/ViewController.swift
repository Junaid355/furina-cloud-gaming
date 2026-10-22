// d:\cloud\ios\FurinaCloud\ViewController.swift
import UIKit
import WebKit

class ViewController: UIViewController, WKUIDelegate, WKNavigationDelegate, WKScriptMessageHandler {
    private var webView: WKWebView!
    private let impactFeedback = UIImpactFeedbackGenerator(style: .medium)
    private let heavyFeedback = UIImpactFeedbackGenerator(style: .heavy)

    override var prefersStatusBarHidden: Bool {
        return true
    }

    override var prefersHomeIndicatorAutoHidden: Bool {
        return true
    }

    override var supportedInterfaceOrientations: UIInterfaceOrientationMask {
        return [.landscapeLeft, .landscapeRight, .portrait]
    }

    override func viewDidLoad() {
        super.viewDidLoad()
        view.backgroundColor = UIColor(red: 0.03, green: 0.05, blue: 0.09, alpha: 1.0)
        setupWebView()
        impactFeedback.prepare()
        heavyFeedback.prepare()
        loadLocalClient()
    }

    private func setupWebView() {
        let contentController = WKUserContentController()
        contentController.add(self, name: "hapticBridge")

        let config = WKWebViewConfiguration()
        config.userContentController = contentController
        config.allowsInlineMediaPlayback = true
        config.mediaTypesRequiringUserActionForPlayback = []
        config.preferences.setValue(true, forKey: "developerExtrasEnabled")

        webView = WKWebView(frame: view.bounds, configuration: config)
        webView.autoresizingMask = [.flexibleWidth, .flexibleHeight]
        webView.uiDelegate = self
        webView.navigationDelegate = self
        webView.scrollView.bounces = false
        webView.scrollView.isScrollEnabled = false
        webView.backgroundColor = .clear
        webView.isOpaque = false

        view.addSubview(webView)
    }

    private func loadLocalClient() {
        // Loads from embedded bundle or local host URL
        let hostUrlString = UserDefaults.standard.string(forKey: "furina_host_url") ?? "http://localhost:8080"
        if let url = URL(string: hostUrlString) {
            let request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalCacheData, timeoutInterval: 10.0)
            webView.load(request)
        }
    }

    func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
        if message.name == "hapticBridge" {
            if let type = message.body as? String {
                switch type {
                case "heavy":
                    heavyFeedback.impactOccurred()
                default:
                    impactFeedback.impactOccurred()
                }
            }
        }
    }
}
