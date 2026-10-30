import json,ssl,urllib.request,zlib,uuid
from pathlib import Path
def entry(i):return (1624453*(0x65F6D+i)+1023920427)%0x8ED7A18D
def pack(data):
 b=bytearray(len(data).to_bytes(4,'little')+data);start=len(b)%0xAAB
 for i in range(len(b)-1,0,-1):j=entry(i+start)%i;b[i],b[j]=b[j],b[i]
 return b
def unpack(data):
 b=bytearray(data);start=len(b)%0xAAB
 for i in range(1,len(b)):j=entry(i+start)%i;b[i],b[j]=b[j],b[i]
 n=int.from_bytes(b[:4],'little');assert n<=len(b)-4
 return b[4:4+n]
import argparse
parser=argparse.ArgumentParser(description="Credits 123 Tarcoins to an explicitly named disposable SPT profile.")
parser.add_argument('--runtime',required=True)
parser.add_argument('--profile',required=True)
args=parser.parse_args()
session=args.profile
root=Path(args.runtime)/'user/profiles'
fixture=json.loads((root/(session+'.json')).read_text(encoding='utf-8-sig'))
assert fixture['info']['username'].startswith('LocalBuildTest'), 'Refusing to credit a non-test account'

def balance(id):return json.loads((root/(id+'.json')).read_text(encoding='utf-8-sig'))['characters']['pmc'].get('TarCoinBalance',0)
def send(amount,requestid, sid=session):
 headers={'Content-Type':'application/json'}
 if sid:headers['Cookie']='PHPSESSID='+sid
 req=urllib.request.Request('https://127.0.0.1:6969/freetarcoins/add',data=pack(zlib.compress(json.dumps({'amount':amount,'requestId':requestid}).encode())),headers=headers)
 with urllib.request.urlopen(req,context=ssl._create_unverified_context(),timeout=20) as resp:data=resp.read()
 data=unpack(data)
 try:data=zlib.decompress(data)
 except zlib.error:pass
 return json.loads(data)
initial=balance(session);others={p.stem:balance(p.stem) for p in root.glob('*.json') if p.stem!=session};rid=str(uuid.uuid4())
a=send(123,rid);print('Credit',a);assert a['Success'] and a['Balance']==initial+123
b=send(123,rid);assert b==a
assert not send(124,rid)['Success']
for invalid in [0,-1,2147483647]:assert not send(invalid,str(uuid.uuid4()))['Success']
assert not send(1,str(uuid.uuid4()),'000000000000000000000001')['Success']
try:
 result=send(1,str(uuid.uuid4()),None)
 assert not result.get('Success',False),result
except urllib.error.HTTPError as e:assert e.code in (400,401,403,500),e.code
assert balance(session)==initial+123
assert all(balance(profile)==value for profile,value in others.items())
print('PASS: real credit, duplicate replay, changed payload, invalid amounts/overflow, unknown/missing session, disk persistence, personal balance unchanged')
