from flask import Flask, jsonify, request
from neo4j import GraphDatabase
from flask_cors import CORS

# 1. Setup the Web Server
app = Flask(__name__)
CORS(app)

# 2. Setup the Connection to your Neo4j "Brain"
# The URI is the 'Connection URI' you saw in Neo4j Desktop
URI = "bolt://localhost:7687"
USER = "neo4j"
PASSWORD = "hetoinet" 

driver = GraphDatabase.driver(URI, auth=(USER, PASSWORD))

# 3. Create the "Librarian" Logic
@app.route('/get_neighbors/<node_id>')
def get_neighbors(node_id):
    """When Unity sends an ID, this finds its neighbors in Hetionet"""
    with driver.session() as session:
        # This is a Cypher query. It finds 10 things connected to our node.
        query = """
        MATCH (n {id: $id})-[r]-(m)
        RETURN m.id AS id, m.name AS name, m.kind AS type
        LIMIT 10
        """
        results = session.run(query, id=node_id)
        
        # Format the data into a list Unity can understand
        data = []
        for record in results:
            data.append({
                "id": record["id"],
                "name": record["name"],
                "type": record["type"]
            })
        
        return jsonify(data)

# 4. Start the Bridge
if __name__ == '__main__':
    print("Bridge is active! Waiting for Unity...")
    app.run(host='0.0.0.0', port=5000)