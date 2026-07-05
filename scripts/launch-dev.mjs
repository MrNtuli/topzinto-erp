import { spawn } from 'node:child_process'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const isWin = process.platform === 'win32'

const child = isWin
  ? spawn(
      'powershell',
      ['-ExecutionPolicy', 'Bypass', '-File', path.join(root, 'scripts', 'start-dev.ps1')],
      { stdio: 'inherit', cwd: root, shell: false },
    )
  : spawn('bash', [path.join(root, 'scripts', 'start-dev.sh')], {
      stdio: 'inherit',
      cwd: root,
      shell: false,
    })

child.on('exit', (code) => process.exit(code ?? 0))
child.on('error', (err) => {
  console.error(err.message)
  process.exit(1)
})
