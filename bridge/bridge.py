from flask import Flask, jsonify
from neo4j import GraphDatabase
from flask_cors import CORS
from urllib.parse import unquote # For decoding URL symbols

app = Flask(__name__)
CORS(app)

# Database Credentials
URI = "bolt://localhost:7687"
USER = "neo4j"
PASSWORD = "hetoinet" 

driver = GraphDatabase.driver(URI, auth=(USER, PASSWORD))

# Use <path:node_id> to capture the full ID including colons/slashes
@app.route('/get_neighbors/<path:node_id>')
def get_neighbors(node_id):
    # CRITICAL: Decode the ID from Unity (e.g., %3a%3a becomes ::)
    decoded_id = unquote(node_id)
    
    print(f"Unity requested: {node_id}")
    print(f"Searching Neo4j for decoded ID: {decoded_id}")

    with driver.session() as session:
        # We use $id as a parameter to prevent injection and handle strings correctly
        query = """
        MATCH (n {id: $id})-[r]-(m)
        RETURN m.id AS id, m.name AS name, m.kind AS type
        LIMIT 10
        """
        
        results = session.run(query, id=decoded_id)
        data = []
        
        for record in results:
            data.append({
                "id": record["id"],
                "name": record["name"],
                "type": record["type"]
            })
        
        print(f"Found {len(data)} neighbors.")
        return jsonify(data)

if __name__ == '__main__':
    print("Bridge is active! Waiting for Unity...")
    app.run(host='0.0.0.0', port=5000, debug=True)