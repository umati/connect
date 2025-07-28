# ========================================================================
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover
# ========================================================================

from flask import Flask, Response

from helper.xml_state import XmlState


def create_app(xml_state: XmlState) -> Flask:
    app = Flask(__name__)

    @app.route("/")
    def root():
        return "MTConnect Server is running. Use /current to get the current XML data."

    @app.route("/current")
    def current():
        xml_str = xml_state.to_string()
        return Response(xml_str, mimetype="application/xml")

    @app.route("/sample")
    def sample():
        from_seq = request.args.get("from", default="1")
        count = request.args.get("count", default="100")

        # You can log or use these if needed
        print(f"Requested from={from_seq}, count={count}")
        xml_str = xml_state.to_string()
        return Response(xml_str, mimetype="application/xml")

    return app
