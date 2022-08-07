import hasha from 'hasha'

import {statsd} from '../../../statsd'
import {config} from '../../../config'

import {trackEvent} from './ga'

import {collectHeartbeat} from './collector/heartbeat'
import {collectInstance} from './collector/instance'

export type BaseParams = {
  builtAt: string
  receivedAt: string
  sceneName: string
  sdk: string
  unity: string
  worldAuthor: string
  worldName: string
  ip: string
}

export type Collector = (tags: Record<string, string>) => void

export const collect = async (ip: string, name: string, tags: Record<string, string>) => {
  if (config.statsd.enabled) {
    switch (name.toLowerCase()) {
      case 'heartbeat':
        collectHeartbeat(tags)
        break

      case 'instance':
        collectInstance(tags)
        break

      default:
        statsd.increment(name, tags)
    }
  }

  if (config.ga.enabled) {
    const userid = hasha(ip, {algorithm: 'sha256'})

    const tracks = Object.entries(tags).map(([key, value]) => {
      return trackEvent(userid, ip, 'com.decentm.vrchat.metrics.event', name, key, value)
    })

    await Promise.all(tracks)
  }
}
