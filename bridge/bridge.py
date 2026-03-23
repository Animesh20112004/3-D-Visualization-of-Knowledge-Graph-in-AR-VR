from flask import Flask, jsonify, request
from neo4j import GraphDatabase
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

URI = "bolt://localhost:7687"
USER = "neo4j"
PASSWORD = "hetoinet" 

driver = GraphDatabase.driver(URI, auth=(USER, PASSWORD))

@app.route('/get_neighbors/<node_id>')
def get_neighbors(node_id):
    with driver.session() as session:
        query = """
        MATCH (n {id: $id})-[r]-(m)
        RETURN m.id AS id, m.name AS name, m.kind AS type
        LIMIT 10
        """
        results = session.run(query, id=node_id)
        data = []
        for record in results:
            data.append({
                "id": record["id"],
                "name": record["name"],
                "type": record["type"]
            })
        
        return jsonify(data)

if __name__ == '__main__':
    print("Bridge is active! Waiting for Unity...")
    app.run(host='0.0.0.0', port=5000)